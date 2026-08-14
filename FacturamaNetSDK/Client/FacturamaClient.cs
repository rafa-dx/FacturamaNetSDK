using FacturamaNetSDK.Configuration;
using FacturamaNetSDK.Endpoints;
using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using Microsoft.Extensions.Logging;

namespace FacturamaNetSDK.Client;

/// <summary>
/// Cliente principal del SDK de Facturama.
/// </summary>
/// <remarks>
/// Está diseñado para instanciarse <b>una sola vez</b> y reutilizarse durante toda la vida
/// de la aplicación: mantiene el pool de conexiones y el estado del circuit breaker.
/// Crear una instancia por petición agota los sockets disponibles.
/// </remarks>
public sealed class FacturamaClient : IDisposable
{
    private readonly FacturamaHttpClient[] _httpClients;
    private bool _disposed;

    /// <summary>
    /// Operaciones CFDI — API Web (/api/3/cfdis).
    /// </summary>
    public ICfdiEndpoint Cfdi { get; }

    /// <summary>
    /// Operaciones CFDI — API Lite (/api-lite/{version}/cfdis).
    /// </summary>
    public ICfdiLiteEndpoint CfdiLite { get; }

    /// <summary>
    /// Operaciones de clientes (/Client).
    /// </summary>
    public IClientEndpoint Clients { get; }

    /// <summary>
    /// Operaciones de productos (/Product).
    /// </summary>
    public IProductEndpoint Products { get; }

    /// <summary>
    /// Operaciones de catálogos (/catalogs).
    /// </summary>  
    public ICatalogEndpoint Catalogs { get; }

    /// <summary>
    /// Operaciones de retenciones (/api/retentions).
    /// </summary>
    public IRetentionEndpoint Retentions { get; }

    /// <summary>
    /// Operaciones del perfil fiscal (contribuyente) de la cuenta.
    /// </summary>
    public ITaxEntityEndpoint TaxEntity { get; }

    /// <summary>
    /// Operaciones de sucursales del perfil fiscal.
    /// </summary>
    public IBranchOfficeEndpoint BranchOffices { get; }

    /// <summary>
    /// Operaciones de series de folios, asociadas a una sucursal.
    /// </summary>
    public ISeriesEndpoint Series { get; }

    /// <summary>
    /// Operaciones del plan de suscripción de la cuenta.
    /// </summary>
    public ISubscriptionPlanEndpoint SubscriptionPlan { get; }

    /// <summary>
    /// Inicializa el cliente con credenciales y configuración por defecto (Sandbox).
    /// </summary>
    public FacturamaClient(string username, string password, ILogger? logger = null)
        : this(options =>
        {
            options.Username = username;
            options.Password = password;
        }, logger)
    { }

    /// <summary>
    /// Inicializa el cliente con configuración avanzada.
    /// </summary>
    public FacturamaClient(Action<FacturamaOptions> configure, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new FacturamaOptions();
        configure(options);
        options.Validate();

        var apiLitePrefix = $"api-lite/{(int)options.ApiLiteVersion}";
        var factory = new FacturamaHttpClientFactory(options, logger);

        var webClient = factory.CreateApiClient("3");
        var liteVersioned = factory.CreateApiClient(apiLitePrefix);
        var liteClient = factory.CreateApiClient("api-lite");
        var rootClient = factory.CreateRootClient();
        var retentionClient = factory.CreateApiClient("api");

        _httpClients = new[] { webClient, liteVersioned, liteClient, rootClient, retentionClient };

        Cfdi = new CfdiEndpoint(webClient);
        CfdiLite = new CfdiLiteEndpoint(liteVersioned, liteClient, rootClient, webClient);
        Clients = new ClientEndpoint(rootClient);
        Products = new ProductEndpoint(rootClient);
        Catalogs = new CatalogEndpoint(rootClient);
        Retentions = new RetentionEndpoint(retentionClient);
        TaxEntity = new TaxEntityEndpoint(rootClient);
        BranchOffices = new BranchOfficeEndpoint(rootClient);
        Series = new SeriesEndpoint(rootClient);
        SubscriptionPlan = new SubscriptionPlanEndpoint(rootClient);
    }

    /// <summary>
    /// Libera los clientes HTTP subyacentes y sus conexiones.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var httpClient in _httpClients)
            httpClient.Dispose();

        _disposed = true;
    }
}