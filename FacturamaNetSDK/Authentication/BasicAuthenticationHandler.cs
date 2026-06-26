using System.Net.Http.Headers;

namespace FacturamaNetSDK.Authentication;

/// <summary>
/// Handler que agrega el header de autenticación Basic Auth en cada petición.
/// </summary>
internal sealed class BasicAuthenticationHandler : DelegatingHandler
{
    private readonly string _encodedCredentials;

    internal BasicAuthenticationHandler(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("El username no puede estar vacío.", nameof(username));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("El password no puede estar vacío.", nameof(password));

        _encodedCredentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _encodedCredentials);
        return base.SendAsync(request, cancellationToken);
    }
}

