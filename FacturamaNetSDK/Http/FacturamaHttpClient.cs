using FacturamaNetSDK.Exceptions;
using FacturamaNetSDK.Serialization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FacturamaNetSDK.Http;

/// <summary>
/// Cliente HTTP base para comunicarse con la API de Facturama.
/// </summary>
internal sealed class FacturamaHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptionsFactory.Default;

    internal FacturamaHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    internal async Task<TResponse> GetAsync<TResponse>(
        string endpoint,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<TResponse>(response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new FacturamaConnectionException(ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FacturamaTimeoutException(ex);
        }
    }

    internal async Task<TResponse> PostAsync<TResponse>(
        string endpoint,
        object request,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);

        try
        {
            using var content = Serialize(request);

            var response = await _httpClient.PostAsync(url, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<TResponse>(response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new FacturamaConnectionException(ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FacturamaTimeoutException(ex);
        }
    }

    internal async Task<TResponse> PutAsync<TResponse>(
        string endpoint,
        object request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = Serialize(request);

            var response = await _httpClient.PutAsync(endpoint, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<TResponse>(response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new FacturamaConnectionException(ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FacturamaTimeoutException(ex);
        }
    }

    internal async Task DeleteAsync(
        string endpoint,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);

        try
        {
            var response = await _httpClient.DeleteAsync(url, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                await ThrowFacturamaExceptionAsync(response, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new FacturamaConnectionException(ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FacturamaTimeoutException(ex);
        }
    }

    internal async Task<TResponse> DeleteAsync<TResponse>(
        string endpoint,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);

        try
        {
            var response = await _httpClient.DeleteAsync(url, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<TResponse>(response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new FacturamaConnectionException(ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FacturamaTimeoutException(ex);
        }
    }

    internal async Task<byte[]> GetBytesAsync(
        string endpoint,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                await ThrowFacturamaExceptionAsync(response, cancellationToken).ConfigureAwait(false);

            return await response.Content.ReadAsByteArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new FacturamaConnectionException(ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FacturamaTimeoutException(ex);
        }
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

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(json))
                return default!;

            try
            {
                return JsonSerializer.Deserialize<T>(json, _jsonOptions)
                    ?? default!;
            }
            catch (JsonException ex)
            {
                throw new FacturamaException("Error al deserializar la respuesta de la API.", ex);
            }
        }

        await ThrowFacturamaExceptionAsync(response, cancellationToken).ConfigureAwait(false);
        return default!;
    }

    private async Task ThrowFacturamaExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                new FacturamaAuthenticationException(),

            HttpStatusCode.NotFound =>
                new FacturamaNotFoundException(response.RequestMessage?.RequestUri?.ToString() ?? string.Empty),

            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity =>
                new FacturamaValidationException(content),

            HttpStatusCode.TooManyRequests =>
                new FacturamaRateLimitException(),

            HttpStatusCode.InternalServerError =>
                new FacturamaServerException(500),

            _ when (int)response.StatusCode >= 500 =>
                new FacturamaServerException((int)response.StatusCode),

            _ =>
                new FacturamaException($"Error inesperado: {(int)response.StatusCode}", (int)response.StatusCode)
        };
    }

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