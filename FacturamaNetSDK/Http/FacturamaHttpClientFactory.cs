using FacturamaNetSDK.Authentication;
using FacturamaNetSDK.Configuration;
using FacturamaNetSDK.Internal;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Extensions.Http;

namespace FacturamaNetSDK.Http;

/// <summary>
/// Construye instancias configuradas de <see cref="FacturamaHttpClient"/>.
/// </summary>
internal static class FacturamaHttpClientFactory
{
    /// <summary>
    /// Cliente para rutas raíz: /Client, /Product, /catalogs, /customers
    /// </summary>
    internal static FacturamaHttpClient CreateRootClient(
        FacturamaOptions options,
        ILogger? logger = null) =>
        Create(options, pathPrefix: null, logger);

    /// <summary>
    /// Cliente para rutas versionadas: /api/3/cfdis, /api/2/retenciones, /api-lite/{version}/cfdis
    /// </summary>
    internal static FacturamaHttpClient CreateApiClient(
        FacturamaOptions options,
        string pathPrefix,
        ILogger? logger = null) =>
        Create(options, pathPrefix, logger);

    // -------------------------------------------------------------------------

    private static FacturamaHttpClient Create(
        FacturamaOptions options,
        string? pathPrefix,
        ILogger? logger)
    {
        // Usamos NullLogger si no se proporcionó logger — nunca será null dentro de los callbacks
        var log = logger ?? NullLogger.Instance;

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, attempt, context) =>
                    log.LogWarning(
                        "Reintento {Attempt}/3 en {Seconds}s — {Reason}",
                        attempt,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? $"HTTP {(int?)outcome.Result?.StatusCode}"));

        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                    log.LogError(
                        "Circuit breaker abierto por {Seconds}s — {Reason}",
                        duration.TotalSeconds,
                        outcome.Exception?.Message ?? $"HTTP {(int?)outcome.Result?.StatusCode}"),
                onReset: () =>
                    log.LogInformation("Circuit breaker cerrado — reanudando peticiones"),
                onHalfOpen: () =>
                    log.LogInformation("Circuit breaker en half-open — probando conexión"));

        var policy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

        var authHandler = new BasicAuthenticationHandler(options.Username, options.Password)
        {
            InnerHandler = new PolicyHttpMessageHandler(policy)
            {
                InnerHandler = new HttpClientHandler()
            }
        };

        DelegatingHandler pipeline = logger is not null
            ? new LoggingHandler(logger) { InnerHandler = authHandler }
            : authHandler;

        var baseUrl = string.IsNullOrEmpty(pathPrefix)
            ? $"{options.BaseUrl}/"
            : $"{options.BaseUrl}/{pathPrefix}/";

        var httpClient = new HttpClient(pipeline)
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = options.Timeout
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(SdkVersion.UserAgent);

        return new FacturamaHttpClient(httpClient);
    }
}