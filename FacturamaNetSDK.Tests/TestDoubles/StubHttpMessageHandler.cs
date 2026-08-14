using System.Net;
using System.Text;

namespace FacturamaNetSDK.Tests.TestDoubles;

/// <summary>
/// Handler de prueba: no toca la red. Registra las peticiones recibidas y
/// devuelve (o lanza) lo que el caso de prueba configure.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    internal StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    internal List<RecordedRequest> Requests { get; } = new();

    internal RecordedRequest LastRequest => Requests[^1];

    internal static StubHttpMessageHandler Returns(HttpStatusCode status, string body = "") =>
        new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });

    internal static StubHttpMessageHandler ReturnsResponse(HttpResponseMessage response) =>
        new(_ => response);

    internal static StubHttpMessageHandler Throws(Exception exception) =>
        new(_ => throw exception);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        Requests.Add(new RecordedRequest(
            request.Method,
            request.RequestUri!,
            request.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
            body));

        var response = _responder(request);

        // HttpClientHandler asocia la petición a la respuesta; el SDK lee RequestMessage
        // para construir FacturamaNotFoundException, así que el stub debe hacer lo mismo.
        response.RequestMessage ??= request;

        return response;
    }
}

/// <summary>
/// Copia inmutable de una petición, capturada antes de que el SDK la libere.
/// </summary>
internal sealed record RecordedRequest(
    HttpMethod Method,
    Uri Uri,
    IReadOnlyDictionary<string, string[]> Headers,
    string? Body)
{
    internal string? Header(string name) =>
        Headers.TryGetValue(name, out var values) ? values.FirstOrDefault() : null;
}
