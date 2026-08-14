using FacturamaNetSDK.Authentication;
using FacturamaNetSDK.Configuration;
using FacturamaNetSDK.Internal;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace FacturamaNetSDK.Http;

/// <summary>
/// Construye instancias configuradas de <see cref="FacturamaHttpClient"/>.
/// Todos los clientes creados por la misma instancia comparten el circuit breaker,
/// de modo que la protección aplica a la cuenta completa y no a cada ruta por separado.
/// </summary>
internal sealed class FacturamaHttpClientFactory
{
    // ⚠️ a definir con el equipo: umbrales del circuit breaker.
    private const int FailuresBeforeBreaking = 5;
    private static readonly TimeSpan BreakDuration = TimeSpan.FromSeconds(30);

    // ⚠️ a definir con el equipo: margen extra sobre el presupuesto total calculado,
    // para que el techo de HttpClient nunca corte antes que el timeout por intento.
    private static readonly TimeSpan SafetyMargin = TimeSpan.FromSeconds(5);

    private readonly FacturamaOptions _options;
    private readonly ILogger? _logger;
    private readonly Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> _policySelector;
    private readonly TimeSpan _totalBudget;

    internal FacturamaHttpClientFactory(FacturamaOptions options, ILogger? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _policySelector = BuildPolicySelector(options, logger ?? NullLogger.Instance);
        _totalBudget = CalculateTotalBudget(options);
    }

    /// <summary>
    /// Cliente para rutas raíz: /Client, /Product, /catalogs, /customers
    /// </summary>
    internal FacturamaHttpClient CreateRootClient() => Create(pathPrefix: null);

    /// <summary>
    /// Cliente para rutas versionadas: /api/3/cfdis, /api/2/retenciones, /api-lite/{version}/cfdis
    /// </summary>
    internal FacturamaHttpClient CreateApiClient(string pathPrefix) => Create(pathPrefix);

    // -------------------------------------------------------------------------

    private FacturamaHttpClient Create(string? pathPrefix)
    {
        var authHandler = new BasicAuthenticationHandler(_options.Username, _options.Password)
        {
            InnerHandler = new PolicyHttpMessageHandler(_policySelector)
            {
                InnerHandler = new HttpClientHandler()
            }
        };

        DelegatingHandler pipeline = _logger is not null
            ? new LoggingHandler(_logger) { InnerHandler = authHandler }
            : authHandler;

        var httpClient = new HttpClient(pipeline)
        {
            BaseAddress = new Uri(BuildBaseUrl(pathPrefix)),
            // Techo total de la operación completa. El timeout por intento lo aplica Polly:
            // si este valor fuera igual a options.Timeout, cortaría los reintentos a medias.
            Timeout = _totalBudget
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(SdkVersion.UserAgent);

        return new FacturamaHttpClient(httpClient);
    }

    private string BuildBaseUrl(string? pathPrefix) =>
        string.IsNullOrEmpty(pathPrefix)
            ? $"{_options.BaseUrl}/"
            : $"{_options.BaseUrl}/{pathPrefix}/";

    // -------------------------------------------------------------------------
    // Políticas de resiliencia
    // -------------------------------------------------------------------------

    private static Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> BuildPolicySelector(
        FacturamaOptions options,
        ILogger log)
    {
        // El breaker y el timeout por intento siempre se construyen: protegen el servicio
        // con independencia de que los reintentos estén activos o no.
        var perAttemptTimeout = BuildPerAttemptTimeout(options, log);
        var circuitBreaker = BuildCircuitBreaker(log);
        var withoutRetry = Policy.WrapAsync(circuitBreaker, perAttemptTimeout);

        if (!options.Retry.Enabled)
            return _ => withoutRetry;

        var withRetry = Policy.WrapAsync(BuildRetry(options, log), circuitBreaker, perAttemptTimeout);

        // Solo reintentan los verbos habilitados; el resto usa breaker + timeout.
        return request => options.Retry.ShouldRetry(request.Method) ? withRetry : withoutRetry;
    }

    private static IAsyncPolicy<HttpResponseMessage> BuildPerAttemptTimeout(
        FacturamaOptions options,
        ILogger log) =>
        Policy.TimeoutAsync<HttpResponseMessage>(
            options.Timeout,
            TimeoutStrategy.Optimistic,
            onTimeoutAsync: (_, timespan, _, _) =>
            {
                log.LogWarning("Intento abortado por timeout de {Seconds}s", timespan.TotalSeconds);
                return Task.CompletedTask;
            });

    private static IAsyncPolicy<HttpResponseMessage> BuildCircuitBreaker(ILogger log) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: FailuresBeforeBreaking,
                durationOfBreak: BreakDuration,
                onBreak: (outcome, duration) =>
                    log.LogError(
                        "Circuit breaker abierto por {Seconds}s — {Reason}",
                        duration.TotalSeconds,
                        Describe(outcome)),
                onReset: () =>
                    log.LogInformation("Circuit breaker cerrado — reanudando peticiones"),
                onHalfOpen: () =>
                    log.LogInformation("Circuit breaker en half-open — probando conexión"));

    private static IAsyncPolicy<HttpResponseMessage> BuildRetry(
        FacturamaOptions options,
        ILogger log) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: options.Retry.MaxRetries,
                sleepDurationProvider: attempt => BackoffDelay(options.Retry, attempt),
                onRetry: (outcome, timespan, attempt, _) =>
                    log.LogWarning(
                        "Reintento {Attempt}/{MaxRetries} en {Seconds}s — {Reason}",
                        attempt,
                        options.Retry.MaxRetries,
                        timespan.TotalSeconds,
                        Describe(outcome)));

    private static string Describe(DelegateResult<HttpResponseMessage> outcome) =>
        outcome.Exception?.Message ?? $"HTTP {(int?)outcome.Result?.StatusCode}";

    /// <summary>
    /// Backoff exponencial: <c>BaseDelay * 2^(intento-1)</c>.
    /// <c>BaseDelay</c> es el multiplicador del primer intento, no la base del exponente.
    /// </summary>
    internal static TimeSpan BackoffDelay(RetryOptions retry, int attempt) =>
        retry.BaseDelay * Math.Pow(2, attempt - 1);

    /// <summary>
    /// Presupuesto total de la operación: todos los intentos más las esperas del backoff.
    /// </summary>
    internal static TimeSpan CalculateTotalBudget(FacturamaOptions options)
    {
        if (!options.Retry.Enabled)
            return options.Timeout + SafetyMargin;

        var backoff = TimeSpan.Zero;
        for (var attempt = 1; attempt <= options.Retry.MaxRetries; attempt++)
            backoff += BackoffDelay(options.Retry, attempt);

        var attempts = options.Retry.MaxRetries + 1;
        return (options.Timeout * attempts) + backoff + SafetyMargin;
    }
}
