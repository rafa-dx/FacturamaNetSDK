using FacturamaNetSDK.Exceptions;
using FacturamaNetSDK.Internal;
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

            return await response.Content.ReadByteArrayAsync(cancellationToken)
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

            // En net5+ el timeout de HttpClient se manifiesta como TaskCanceledException con
            // TimeoutException en la cadena de excepciones internas: es la señal determinista.
            // En netstandard2.0 (p.ej. .NET Framework) esa interna no llega, así que ahí el
            // timeout se detecta por el respaldo de abajo: se canceló sin que el consumidor
            // lo pidiera.
            TaskCanceledException ex when HasTimeoutInChain(ex)
                => new FacturamaTimeoutException(ex),

            TaskCanceledException ex when !cancellationToken.IsCancellationRequested
                => new FacturamaTimeoutException(ex),

            _ => null
        };

    /// <summary>
    /// Busca una <see cref="TimeoutException"/> en toda la cadena de excepciones internas.
    /// No basta con mirar el primer nivel: net8 envuelve el timeout del handler en una
    /// <see cref="TaskCanceledException"/> adicional y deja la señal un nivel más abajo.
    /// </summary>
    private static bool HasTimeoutInChain(Exception exception)
    {
        for (var inner = exception.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner is TimeoutException)
                return true;
        }

        return false;
    }

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

        var json = await response.Content.ReadStringAsync(cancellationToken)
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
        var content = await response.Content.ReadStringAsync(cancellationToken)
            .ConfigureAwait(false);

        throw MapStatusCode(response, content);
    }

    /// <summary>
    /// Códigos que el SDK traduce. Se comparan como enteros porque 422 y 429 no existen en el
    /// enum <see cref="HttpStatusCode"/> de netstandard2.0.
    /// </summary>
    private static class Status
    {
        internal const int BadRequest = 400;
        internal const int Unauthorized = 401;
        internal const int NotFound = 404;
        internal const int UnprocessableEntity = 422;
        internal const int TooManyRequests = 429;
        internal const int FirstServerError = 500;
    }

    private FacturamaException MapStatusCode(HttpResponseMessage response, string content) =>
        (int)response.StatusCode switch
        {
            Status.Unauthorized =>
                new FacturamaAuthenticationException(),

            Status.NotFound =>
                new FacturamaNotFoundException(response.RequestMessage?.RequestUri?.ToString() ?? string.Empty),

            Status.BadRequest or Status.UnprocessableEntity =>
                new FacturamaValidationException(content),

            Status.TooManyRequests =>
                new FacturamaRateLimitException(RetryAfter(response)),

            var status when status >= Status.FirstServerError =>
                new FacturamaServerException(status),

            var status =>
                new FacturamaException($"Error inesperado: {status}", status)
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
