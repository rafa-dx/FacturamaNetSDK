using FacturamaNetSDK.Endpoints;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Models.Product.Request;
using FacturamaNetSDK.Tests.TestDoubles;
using System.Net;

namespace FacturamaNetSDK.Tests.Endpoints;

public sealed class ProductEndpointTests
{
    private const string PageJson = """
        {
          "recordsTotal": 120,
          "recordsFiltered": 2,
          "data": [
            { "Id": "abc", "Name": "Silla", "Price": 100.50 },
            { "Id": "def", "Name": "Mesa",  "Price": 250.00 }
          ]
        }
        """;

    private static (ProductEndpoint Endpoint, FacturamaHttpClient Client) CreateSut(
        StubHttpMessageHandler handler)
    {
        var client = new FacturamaHttpClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://apisandbox.facturama.mx/") });
        return (new ProductEndpoint(client), client);
    }

    // -------------------------------------------------------------------------
    // Listado paginado — el sobre es un objeto único, no una colección
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListAsync_DeserializaElSobreDePaginacion()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, PageJson);
        var (endpoint, client) = CreateSut(handler);
        using (client)
        {
            var page = await endpoint.ListAsync();

            Assert.Equal(120, page.RecordsTotal);
            Assert.Equal(2, page.RecordsFiltered);
            Assert.Equal(2, page.Data.Count);
            Assert.Equal("Silla", page.Data[0].Name);
            Assert.Equal(250.00m, page.Data[1].Price);
        }
    }

    /// <summary>
    /// Las propiedades son PascalCase en C# pero el API espera los nombres de
    /// DataTables en minúsculas; el mapeo lo resuelve <c>JsonPropertyName</c>.
    /// </summary>
    [Fact]
    public async Task ListAsync_EnviaLosParametrosConElNombreDelApi()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, PageJson);
        var (endpoint, client) = CreateSut(handler);
        using (client)
        {
            await endpoint.ListAsync(new QueryOptions { Start = 20, Length = 50, Search = "silla" });

            var query = handler.LastRequest.Uri.Query;
            Assert.Contains("start=20", query);
            Assert.Contains("length=50", query);
            Assert.Contains("search=silla", query);
            Assert.DoesNotContain("Start=", query);
        }
    }

    [Fact]
    public async Task ListAsync_SinFiltros_NoEnviaQueryString()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, PageJson);
        var (endpoint, client) = CreateSut(handler);
        using (client)
        {
            await endpoint.ListAsync();

            Assert.Equal(string.Empty, handler.LastRequest.Uri.Query);
            Assert.EndsWith("/products", handler.LastRequest.Uri.AbsolutePath);
        }
    }

    [Fact]
    public async Task ListAsync_ConBusquedaVacia_OmiteElParametro()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, PageJson);
        var (endpoint, client) = CreateSut(handler);
        using (client)
        {
            await endpoint.ListAsync(new QueryOptions { Search = string.Empty });

            Assert.DoesNotContain("search=", handler.LastRequest.Uri.Query);
        }
    }

    // -------------------------------------------------------------------------
    // Idempotencia en la escritura
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_PropagaLaClaveDeIdempotencia()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Id":"abc","Name":"Silla"}""");
        var (endpoint, client) = CreateSut(handler);
        using (client)
        {
            await endpoint.CreateAsync(new ProductRequest { Name = "Silla" }, idempotencyKey: "clave-1");

            Assert.Equal("clave-1", handler.LastRequest.Header("Idempotency-Key"));
        }
    }

    [Fact]
    public async Task CreateAsync_SinClave_GeneraUna()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Id":"abc","Name":"Silla"}""");
        var (endpoint, client) = CreateSut(handler);
        using (client)
        {
            await endpoint.CreateAsync(new ProductRequest { Name = "Silla" });

            Assert.True(Guid.TryParse(handler.LastRequest.Header("Idempotency-Key"), out _));
        }
    }

    // -------------------------------------------------------------------------
    // Rutas
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("abc")]
    [InlineData("id-con-guiones")]
    public async Task GetAsync_ConstruyeLaRutaDelRecurso(string id)
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Id":"abc","Name":"Silla"}""");
        var (endpoint, client) = CreateSut(handler);
        using (client)
        {
            await endpoint.GetAsync(id);

            Assert.Equal($"/product/{id}", handler.LastRequest.Uri.AbsolutePath);
            Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        }
    }

    [Fact]
    public async Task DeleteAsync_UsaElVerboDelete()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Id":"abc"}""");
        var (endpoint, client) = CreateSut(handler);
        using (client)
        {
            await endpoint.DeleteAsync("abc");

            Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
            Assert.Equal("/product/abc", handler.LastRequest.Uri.AbsolutePath);
        }
    }

    [Fact]
    public async Task UpdateAsync_UsaElVerboPut()
    {
        var handler = StubHttpMessageHandler.Returns(HttpStatusCode.OK, """{"Id":"abc"}""");
        var (endpoint, client) = CreateSut(handler);
        using (client)
        {
            await endpoint.UpdateAsync("abc", new ProductRequest { Name = "Silla" });

            Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        }
    }

    // -------------------------------------------------------------------------
    // Contrato del constructor
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_ConClienteNulo_Lanza()
    {
        Assert.Throws<ArgumentNullException>(() => new ProductEndpoint(null!));
    }
}
