using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FacturamaNetSDK.Http;

/// <summary>
/// Handler que registra peticiones y respuestas HTTP en el pipeline.
/// </summary>
internal sealed class LoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    internal LoggingHandler(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogDebug(
            "HTTP {Method} {Uri}",
            request.Method,
            request.RequestUri);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        sw.Stop();

        _logger.LogDebug(
            "HTTP {Method} {Uri} → {StatusCode} ({ElapsedMs}ms)",
            request.Method,
            request.RequestUri,
            (int)response.StatusCode,
            sw.ElapsedMilliseconds);

        return response;
    }
}