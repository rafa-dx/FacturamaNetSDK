using FacturamaNetSDK.Endpoints;
using FacturamaNetSDK.Enums;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Client.Request;
using FacturamaNetSDK.Tests.TestDoubles;
using System.Net;

namespace FacturamaNetSDK.Tests.Endpoints;

/// <summary>
/// Las validaciones de argumento deben lanzarse <b>al invocar</b>, no al esperar la Task.
/// Cada prueba llama al método sin <c>await</c>: si la excepción quedara capturada en la
/// Task, no habría excepción que observar y la prueba fallaría.
/// </summary>
public sealed class ArgumentValidationTests
{
    private static FacturamaHttpClient CreateClient() =>
        new(new HttpClient(StubHttpMessageHandler.Returns(HttpStatusCode.OK, "{}"))
        {
            BaseAddress = new Uri("https://apisandbox.facturama.mx/")
        });

    public static TheoryData<string> IdsInvalidos => new() { "", "   ", "\t" };

    // -------------------------------------------------------------------------
    // CFDI — API Web
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(IdsInvalidos))]
    public void CfdiEndpoint_GetAsync_ConIdInvalido_LanzaSincronicamente(string id)
    {
        using var client = CreateClient();
        var endpoint = new CfdiEndpoint(client);

        Assert.Throws<ArgumentException>(() => { _ = endpoint.GetAsync(id); });
    }

    [Theory]
    [MemberData(nameof(IdsInvalidos))]
    public void CfdiEndpoint_CancelAsync_ConIdInvalido_LanzaSincronicamente(string id)
    {
        using var client = CreateClient();
        var endpoint = new CfdiEndpoint(client);

        Assert.Throws<ArgumentException>(
            () => { _ = endpoint.CancelAsync(id, InvoiceType.Issued); });
    }

    [Fact]
    public void CfdiEndpoint_CreateAsync_ConRequestNulo_LanzaSincronicamente()
    {
        using var client = CreateClient();
        var endpoint = new CfdiEndpoint(client);

        Assert.Throws<ArgumentNullException>(() => { _ = endpoint.CreateAsync(null!); });
    }

    [Fact]
    public void CfdiEndpoint_SendByEmailAsync_ConEmailVacio_LanzaSincronicamente()
    {
        using var client = CreateClient();
        var endpoint = new CfdiEndpoint(client);

        Assert.Throws<ArgumentException>(() => { _ = endpoint.SendByEmailAsync("id-valido", ""); });
    }

    // -------------------------------------------------------------------------
    // CFDI — API Lite
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(IdsInvalidos))]
    public void CfdiLiteEndpoint_GetAsync_ConIdInvalido_LanzaSincronicamente(string id)
    {
        using var client = CreateClient();
        var endpoint = new CfdiLiteEndpoint(client, client, client, client);

        Assert.Throws<ArgumentNullException>(() => { _ = endpoint.GetAsync(id); });
    }

    [Fact]
    public void CfdiLiteEndpoint_CreateAsync_ConRequestNulo_LanzaSincronicamente()
    {
        using var client = CreateClient();
        var endpoint = new CfdiLiteEndpoint(client, client, client, client);

        Assert.Throws<ArgumentNullException>(() => { _ = endpoint.CreateAsync(null!); });
    }

    // -------------------------------------------------------------------------
    // Clientes
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(IdsInvalidos))]
    public void ClientEndpoint_GetAsync_ConIdInvalido_LanzaSincronicamente(string id)
    {
        using var client = CreateClient();
        var endpoint = new ClientEndpoint(client);

        Assert.Throws<ArgumentNullException>(() => { _ = endpoint.GetAsync(id); });
    }

    [Fact]
    public void ClientEndpoint_UpdateAsync_ConRequestNulo_LanzaSincronicamente()
    {
        using var client = CreateClient();
        var endpoint = new ClientEndpoint(client);

        Assert.Throws<ArgumentNullException>(
            () => { _ = endpoint.UpdateAsync("id-valido", null!); });
    }

    [Fact]
    public void ClientEndpoint_ValidateAsync_ConRequestNulo_LanzaSincronicamente()
    {
        using var client = CreateClient();
        var endpoint = new ClientEndpoint(client);

        Assert.Throws<ArgumentNullException>(() => { _ = endpoint.ValidateAsync(null!); });
    }

    // -------------------------------------------------------------------------
    // Series — el caso que además transforma el resultado (ListCoreAsync)
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(IdsInvalidos))]
    public void SeriesEndpoint_ListAsync_ConSucursalInvalida_LanzaSincronicamente(string id)
    {
        using var client = CreateClient();
        var endpoint = new SeriesEndpoint(client);

        Assert.Throws<ArgumentNullException>(() => { _ = endpoint.ListAsync(id); });
    }

    [Fact]
    public void SeriesEndpoint_DeleteAsync_ConSerieVacia_LanzaSincronicamente()
    {
        using var client = CreateClient();
        var endpoint = new SeriesEndpoint(client);

        Assert.Throws<ArgumentNullException>(
            () => { _ = endpoint.DeleteAsync("sucursal-1", ""); });
    }

    // -------------------------------------------------------------------------
    // Sucursales, retenciones y productos
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(IdsInvalidos))]
    public void BranchOfficeEndpoint_GetAsync_ConIdInvalido_LanzaSincronicamente(string id)
    {
        using var client = CreateClient();
        var endpoint = new BranchOfficeEndpoint(client);

        Assert.Throws<ArgumentNullException>(() => { _ = endpoint.GetAsync(id); });
    }

    [Theory]
    [MemberData(nameof(IdsInvalidos))]
    public void RetentionEndpoint_GetAsync_ConIdInvalido_LanzaSincronicamente(string id)
    {
        using var client = CreateClient();
        var endpoint = new RetentionEndpoint(client);

        Assert.Throws<ArgumentException>(() => { _ = endpoint.GetAsync(id); });
    }

    [Theory]
    [MemberData(nameof(IdsInvalidos))]
    public void ProductEndpoint_GetAsync_ConIdInvalido_LanzaSincronicamente(string id)
    {
        using var client = CreateClient();
        var endpoint = new ProductEndpoint(client);

        Assert.Throws<ArgumentException>(() => { _ = endpoint.GetAsync(id); });
    }

    [Theory]
    [MemberData(nameof(IdsInvalidos))]
    public void ProductEndpoint_DeleteAsync_ConIdInvalido_LanzaSincronicamente(string id)
    {
        using var client = CreateClient();
        var endpoint = new ProductEndpoint(client);

        Assert.Throws<ArgumentException>(() => { _ = endpoint.DeleteAsync(id); });
    }

    [Fact]
    public void ProductEndpoint_CreateAsync_ConRequestNulo_LanzaSincronicamente()
    {
        using var client = CreateClient();
        var endpoint = new ProductEndpoint(client);

        Assert.Throws<ArgumentNullException>(() => { _ = endpoint.CreateAsync(null!); });
    }

    // -------------------------------------------------------------------------
    // Contraprueba: con argumentos válidos no se lanza nada antes del await
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ConArgumentosValidos_NoLanzaAntesDelAwait()
    {
        using var client = CreateClient();
        var endpoint = new ProductEndpoint(client);

        var pending = endpoint.GetAsync("abc");

        Assert.NotNull(pending);
        await pending;
    }

    [Fact]
    public async Task ConArgumentosValidos_LaValidacionNoInterfiereConElRequest()
    {
        using var client = CreateClient();
        var endpoint = new ClientEndpoint(client);

        var pending = endpoint.CreateAsync(new ClientRequest());

        await pending;
        Assert.True(pending.IsCompletedSuccessfully);
    }
}
