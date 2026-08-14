using FacturamaNetSDK.Exceptions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Tests.TestDoubles;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace FacturamaNetSDK.Tests.Http;

public sealed class FacturamaHttpClientTests
{
    private static readonly Guid FixedGuid = new("11111111-2222-3333-4444-555555555555");
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 13, 12, 0, 0, TimeSpan.Zero);

    private sealed record Payload(string Name, int Total);

    private static FacturamaHttpClient CreateSut(StubHttpMessageHandler handler) =>
        new(
            new HttpClient(handler) { BaseAddress = new Uri("https://apisandbox.facturama.mx/") },
            newGuid: () => FixedGuid,
            utcNow: () => FixedNow);

    // -------------------------------------------------------------------------
    // Happy path y deserialización
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_DeserializaLaRespuesta()
    {
        var handler = StubHttpMessageHandler.Returns(
            HttpStatusCode.OK, """{"Name":"Producto","Total":42}""");
        using var sut = CreateSut(handler);

        var result = await sut.GetAsync<Payload>("product/1");

        Assert.Equal("Producto", result.Name);
        Assert.Equal(42, result.Total);
    }

    [Fact]
    public async Task GetAsync_DeserializaIgnorandoMayusculas()
    {
        var handler = StubHttpMessageHandler.Returns(
            HttpStatusCode.OK, """{"name":"minusculas","total":7}""");
        using var sut = CreateSut(handler);

        var result = await sut.GetAsync<Payload>("product/1");

        Assert.Equal("minusculas", result.Name);
    }

    [Fact]
    public async Task GetAsync_SinContenido_DevuelveDefault()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.NoContent);
        using var sut = CreateSut(handler);

        var result = await sut.GetAsync<Payload?>("product/1");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_CuerpoVacio_DevuelveDefault()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, "   ");
        using var sut = CreateSut(handler);

        var result = await sut.GetAsync<Payload?>("product/1");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_JsonInvalido_LanzaFacturamaException()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, "{ esto no es json ]");
        using var sut = CreateSut(handler);

        var ex = await Assert.ThrowsAsync<FacturamaException>(
            () => sut.GetAsync<Payload>("product/1"));

        Assert.Contains("deserializar", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Un objeto JSON no puede deserializarse en una colección. Es exactamente el fallo
    /// que producía la firma incorrecta de <c>ProductEndpoint.ListAsync</c>.
    /// </summary>
    [Fact]
    public async Task GetAsync_ObjetoEnLugarDeColeccion_LanzaFacturamaException()
    {
        var handler = StubHttpMessageHandler.Returns(
            HttpStatusCode.OK, """{"recordsTotal":1,"data":[]}""");
        using var sut = CreateSut(handler);

        await Assert.ThrowsAsync<FacturamaException>(
            () => sut.GetAsync<List<Payload>>("products"));
    }

    // -------------------------------------------------------------------------
    // Mapeo de códigos HTTP a excepciones tipadas
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_401_LanzaAuthenticationException()
    {
        using var sut = CreateSut(StubHttpMessageHandler.Returns(HttpStatusCode.Unauthorized));

        var ex = await Assert.ThrowsAsync<FacturamaAuthenticationException>(
            () => sut.GetAsync<Payload>("product/1"));

        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task GetAsync_404_LanzaNotFoundExceptionConLaUrl()
    {
        using var sut = CreateSut(StubHttpMessageHandler.Returns(HttpStatusCode.NotFound));

        var ex = await Assert.ThrowsAsync<FacturamaNotFoundException>(
            () => sut.GetAsync<Payload>("product/999"));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("product/999", ex.ResourceId);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.UnprocessableEntity)]
    public async Task GetAsync_400Y422_LanzanValidationException(HttpStatusCode status)
    {
        using var sut = CreateSut(StubHttpMessageHandler.Returns(status, "RFC inválido"));

        var ex = await Assert.ThrowsAsync<FacturamaValidationException>(
            () => sut.GetAsync<Payload>("product/1"));

        Assert.Equal("RFC inválido", ex.Message);
    }

    [Fact]
    public async Task GetAsync_429_LanzaRateLimitConRetryAfterRelativo()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(string.Empty)
        };
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30));
        using var sut = CreateSut(StubHttpMessageHandler.ReturnsResponse(response));

        var ex = await Assert.ThrowsAsync<FacturamaRateLimitException>(
            () => sut.GetAsync<Payload>("product/1"));

        Assert.Equal(TimeSpan.FromSeconds(30), ex.RetryAfter);
    }

    /// <summary>
    /// Con <c>Retry-After</c> como fecha absoluta, el delta se calcula contra el reloj
    /// inyectado — por eso la prueba es determinista.
    /// </summary>
    [Fact]
    public async Task GetAsync_429_CalculaRetryAfterDesdeFechaConRelojInyectado()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(string.Empty)
        };
        response.Headers.RetryAfter = new RetryConditionHeaderValue(FixedNow.AddSeconds(90));
        using var sut = CreateSut(StubHttpMessageHandler.ReturnsResponse(response));

        var ex = await Assert.ThrowsAsync<FacturamaRateLimitException>(
            () => sut.GetAsync<Payload>("product/1"));

        Assert.Equal(TimeSpan.FromSeconds(90), ex.RetryAfter);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, 500)]
    [InlineData(HttpStatusCode.BadGateway, 502)]
    [InlineData(HttpStatusCode.ServiceUnavailable, 503)]
    public async Task GetAsync_5xx_LanzaServerException(HttpStatusCode status, int expected)
    {
        using var sut = CreateSut(StubHttpMessageHandler.Returns(status));

        var ex = await Assert.ThrowsAsync<FacturamaServerException>(
            () => sut.GetAsync<Payload>("product/1"));

        Assert.Equal(expected, ex.StatusCode);
    }

    [Fact]
    public async Task GetAsync_StatusNoContemplado_LanzaFacturamaExceptionBase()
    {
        using var sut = CreateSut(StubHttpMessageHandler.Returns(HttpStatusCode.Conflict));

        var ex = await Assert.ThrowsAsync<FacturamaException>(
            () => sut.GetAsync<Payload>("product/1"));

        Assert.Equal(409, ex.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Errores de infraestructura
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_HttpRequestException_LanzaConnectionException()
    {
        using var sut = CreateSut(
            StubHttpMessageHandler.Throws(new HttpRequestException("sin red")));

        var ex = await Assert.ThrowsAsync<FacturamaConnectionException>(
            () => sut.GetAsync<Payload>("product/1"));

        Assert.IsType<HttpRequestException>(ex.InnerException);
    }

    [Fact]
    public async Task GetAsync_CircuitoAbierto_LanzaServerException503()
    {
        using var sut = CreateSut(
            StubHttpMessageHandler.Throws(new BrokenCircuitException("circuito abierto")));

        var ex = await Assert.ThrowsAsync<FacturamaServerException>(
            () => sut.GetAsync<Payload>("product/1"));

        Assert.Equal(503, ex.StatusCode);
        Assert.Contains("circuit breaker", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// El timeout por intento de Polly debe llegar al consumidor como timeout del SDK,
    /// no como una excepción de Polly sin traducir.
    /// </summary>
    [Fact]
    public async Task GetAsync_TimeoutDePolly_LanzaTimeoutException()
    {
        using var sut = CreateSut(
            StubHttpMessageHandler.Throws(new TimeoutRejectedException("intento agotado")));

        await Assert.ThrowsAsync<FacturamaTimeoutException>(
            () => sut.GetAsync<Payload>("product/1"));
    }

    [Fact]
    public async Task GetAsync_TimeoutDeHttpClient_LanzaTimeoutException()
    {
        using var sut = CreateSut(StubHttpMessageHandler.Throws(
            new TaskCanceledException("agotado", new TimeoutException())));

        await Assert.ThrowsAsync<FacturamaTimeoutException>(
            () => sut.GetAsync<Payload>("product/1"));
    }

    /// <summary>
    /// Regresión: un timeout que coincide con un token ya cancelado debe seguir
    /// clasificándose como timeout. La señal es la TimeoutException interna, no el token.
    /// </summary>
    [Fact]
    public async Task GetAsync_TimeoutConTokenYaCancelado_SigueSiendoTimeout()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var sut = CreateSut(StubHttpMessageHandler.Throws(
            new TaskCanceledException("agotado", new TimeoutException())));

        await Assert.ThrowsAsync<FacturamaTimeoutException>(
            () => sut.GetAsync<Payload>("product/1", cancellationToken: cts.Token));
    }

    /// <summary>
    /// Una cancelación pedida por el consumidor no es un error del SDK: se propaga tal cual.
    /// </summary>
    [Fact]
    public async Task GetAsync_CancelacionDelConsumidor_PropagaOperationCanceled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var sut = CreateSut(
            StubHttpMessageHandler.Throws(new TaskCanceledException("cancelado")));

        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.GetAsync<Payload>("product/1", cancellationToken: cts.Token));

        Assert.IsNotType<FacturamaTimeoutException>(ex);
    }

    // -------------------------------------------------------------------------
    // Idempotencia
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PostAsync_SinClave_UsaElGeneradorInyectado()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Name":"x","Total":1}""");
        using var sut = CreateSut(handler);

        await sut.PostAsync<Payload>("product", new { Name = "x" });

        Assert.Equal(FixedGuid.ToString(), handler.LastRequest.Header("Idempotency-Key"));
    }

    [Fact]
    public async Task PostAsync_ConClaveExplicita_LaRespeta()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Name":"x","Total":1}""");
        using var sut = CreateSut(handler);

        await sut.PostAsync<Payload>("product", new { Name = "x" }, idempotencyKey: "clave-del-consumidor");

        Assert.Equal("clave-del-consumidor", handler.LastRequest.Header("Idempotency-Key"));
    }

    [Fact]
    public async Task PostAsync_SerializaElCuerpoComoJson()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Name":"x","Total":1}""");
        using var sut = CreateSut(handler);

        await sut.PostAsync<Payload>("product", new { Name = "Servicio" });

        Assert.Contains("\"Name\":\"Servicio\"", handler.LastRequest.Body);
    }

    // -------------------------------------------------------------------------
    // Construcción de URL y query params
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_AgregaQueryParamsCodificados()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Name":"x","Total":1}""");
        using var sut = CreateSut(handler);

        await sut.GetAsync<Payload>("products", new Dictionary<string, string?>
        {
            ["search"] = "café con leche"
        });

        Assert.Contains("search=caf", handler.LastRequest.Uri.Query);
        Assert.DoesNotContain(" ", handler.LastRequest.Uri.Query);
    }

    [Fact]
    public async Task GetAsync_OmiteQueryParamsVaciosONulos()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Name":"x","Total":1}""");
        using var sut = CreateSut(handler);

        await sut.GetAsync<Payload>("products", new Dictionary<string, string?>
        {
            ["keyword"] = "silla",
            ["motive"] = null,
            ["serie"] = "   "
        });

        var query = handler.LastRequest.Uri.Query;
        Assert.Contains("keyword=silla", query);
        Assert.DoesNotContain("motive", query);
        Assert.DoesNotContain("serie", query);
    }

    [Fact]
    public async Task GetAsync_SinQueryParams_NoAgregaInterrogacion()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Name":"x","Total":1}""");
        using var sut = CreateSut(handler);

        await sut.GetAsync<Payload>("products");

        Assert.Equal(string.Empty, handler.LastRequest.Uri.Query);
    }

    // -------------------------------------------------------------------------
    // DELETE sin cuerpo de respuesta
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_Exitoso_NoLanza()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.NoContent);
        using var sut = CreateSut(handler);

        await sut.DeleteAsync("product/1");

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
    }

    [Fact]
    public async Task DeleteAsync_ConError_LanzaExcepcionTipada()
    {
        using var sut = CreateSut(StubHttpMessageHandler.Returns(HttpStatusCode.NotFound));

        await Assert.ThrowsAsync<FacturamaNotFoundException>(() => sut.DeleteAsync("product/1"));
    }

    // -------------------------------------------------------------------------
    // Contrato del constructor y del ciclo de vida
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_ConHttpClientNulo_Lanza()
    {
        Assert.Throws<ArgumentNullException>(() => new FacturamaHttpClient(null!));
    }

    [Fact]
    public async Task Dispose_LiberaElHttpClientSubyacente()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Name":"x","Total":1}""");
        var sut = CreateSut(handler);

        sut.Dispose();

        await Assert.ThrowsAnyAsync<ObjectDisposedException>(
            () => sut.GetAsync<Payload>("product/1"));
    }
}
