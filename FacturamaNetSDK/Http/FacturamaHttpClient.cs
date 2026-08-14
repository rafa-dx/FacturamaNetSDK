using FacturamaNetSDK.Exceptions;
using FacturamaNetSDK.Serialization;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FacturamaNetSDK.Http;

/// <summary>
/// Cliente HTTP base para comunicarse con la API de Facturama.
/// </summary>
internal sealed class FacturamaHttpClient : IDisposable
{
    private const string CircuitOpenMessage =
        "El servicio de Facturama no está disponible temporalmente (circuit breaker abierto). Intenta más tarde.";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptionsFactory.Default;
    private readonly Func<Guid> _newGuid;
    private readonly Func<DateTimeOffset> _utcNow;

    internal FacturamaHttpClient(
        HttpClient httpClient,
        Func<Guid>? newGuid = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _newGuid = newGuid ?? Guid.NewGuid;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    internal Task<TResponse> GetAsync<TResponse>(
        string endpoint,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);

        return ExecuteAsync(async () =>
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<TResponse>(response, cancellationToken)
                .ConfigureAwait(false);
        }, cancellationToken);
    }

    internal Task<TResponse> PostAsync<TResponse>(
        string endpoint,
        object request,
        Dictionary<string, string?>? queryParams = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);

        return ExecuteAsync(async () =>
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = Serialize(request)
            };

            httpRequest.Headers.TryAddWithoutValidation(
                "Idempotency-Key",
                idempotencyKey ?? _newGuid().ToString());

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<TResponse>(response, cancellationToken)
                .ConfigureAwait(false);
        }, cancellationToken);
    }

    internal Task<TResponse> PutAsync<TResponse>(
        string endpoint,
        object request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async () =>
        {
            using var content = Serialize(request);

            using var response = await _httpClient.PutAsync(endpoint, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<TResponse>(response, cancellationToken)
                .ConfigureAwait(false);
        }, cancellationToken);
    }

    internal Task DeleteAsync(
        string endpoint,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);

        return ExecuteAsync(async () =>
        {
            using var response = await _httpClient.DeleteAsync(url, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                await ThrowFacturamaExceptionAsync(response, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    internal Task<TResponse> DeleteAsync<TResponse>(
        string endpoint,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);

        return ExecuteAsync(async () =>
        {
            using var response = await _httpClient.DeleteAsync(url, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<TResponse>(response, cancellationToken)
                .ConfigureAwait(false);
        }, cancellationToken);
    }

    internal Task<byte[]> GetBytesAsync(
        string endpoint,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);

        return ExecuteAsync(async () =>
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                await ThrowFacturamaExceptionAsync(response, cancellationToken).ConfigureAwait(false);

            return await response.Content.ReadAsByteArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }, cancellationToken);
    }

    public void Dispose() => _httpClient.Dispose();

    // -------------------------------------------------------------------------
    // Traducción de excepciones — punto único para todos los verbos
    // -------------------------------------------------------------------------

    private static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken)
    {
        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (Exception ex) when (TranslateException(ex, cancellationToken) is { } translated)
        {
            throw translated;
        }
    }

    private static async Task ExecuteAsync(
        Func<Task> operation,
        CancellationToken cancellationToken)
    {
        try
        {
            await operation().ConfigureAwait(false);
        }
        catch (Exception ex) when (TranslateException(ex, cancellationToken) is { } translated)
        {
            throw translated;
        }
    }

    /// <summary>
    /// Traduce las excepciones de infraestructura a excepciones tipadas del SDK.
    /// Devuelve <c>null</c> cuando la excepción debe propagarse sin cambios
    /// (p.ej. cancelación solicitada por el consumidor).
    /// </summary>
    private static FacturamaException? TranslateException(
        Exception exception,
        CancellationToken cancellationToken) => exception switch
        {
            BrokenCircuitException => new FacturamaServerException(CircuitOpenMessage, 503),

            TimeoutRejectedException ex => new FacturamaTimeoutException(ex),

            HttpRequestException ex => new FacturamaConnectionException(ex),

            // El timeout de HttpClient se manifiesta como TaskCanceledException con
            // TimeoutException interna (net6+). Es la señal determinista; el chequeo del
            // token queda como respaldo para cancelaciones que no son del consumidor.
            TaskCanceledException ex when ex.InnerException is TimeoutException
                => new FacturamaTimeoutException(ex),

            TaskCanceledException ex when !cancellationToken.IsCancellationRequested
                => new FacturamaTimeoutException(ex),

            _ => null
        };

    // -------------------------------------------------------------------------
    // Helpers privados
    // -------------------------------------------------------------------------

    private StringContent Serialize(object request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<T> HandleResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NoContent)
            return default!;

        if (!response.IsSuccessStatusCode)
        {
            await ThrowFacturamaExceptionAsync(response, cancellationToken).ConfigureAwait(false);
            return default!;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        return Deserialize<T>(json);
    }

    private T Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default!;

        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? default!;
        }
        catch (JsonException ex)
        {
            throw new FacturamaException("Error al deserializar la respuesta de la API.", ex);
        }
    }

    private async Task ThrowFacturamaExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        throw MapStatusCode(response, content);
    }

    private FacturamaException MapStatusCode(HttpResponseMessage response, string content) =>
        response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                new FacturamaAuthenticationException(),

            HttpStatusCode.NotFound =>
                new FacturamaNotFoundException(response.RequestMessage?.RequestUri?.ToString() ?? string.Empty),

            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity =>
                new FacturamaValidationException(content),

            HttpStatusCode.TooManyRequests =>
                new FacturamaRateLimitException(RetryAfter(response)),

            HttpStatusCode.InternalServerError =>
                new FacturamaServerException(500),

            _ when (int)response.StatusCode >= 500 =>
                new FacturamaServerException((int)response.StatusCode),

            _ =>
                new FacturamaException($"Error inesperado: {(int)response.StatusCode}", (int)response.StatusCode)
        };

    private TimeSpan? RetryAfter(HttpResponseMessage response) =>
        response.Headers.RetryAfter?.Delta
            ?? (response.Headers.RetryAfter?.Date is { } retryDate
                ? retryDate - _utcNow()
                : null);

    private static string BuildUrl(string endpoint, Dictionary<string, string?>? queryParams)
    {
        if (queryParams is null || queryParams.Count == 0)
            return endpoint;

        var qs = string.Join("&",
            queryParams
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => $"{kvp.Key}={WebUtility.UrlEncode(kvp.Value)}"));

        if (string.IsNullOrEmpty(qs))
            return endpoint;

        var separator = endpoint.Contains('?') ? '&' : '?';
        return $"{endpoint}{separator}{qs}";
    }
}
