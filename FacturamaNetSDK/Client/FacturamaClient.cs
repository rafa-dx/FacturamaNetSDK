using FacturamaNetSDK.Configuration;
using FacturamaNetSDK.Endpoints;
using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using Microsoft.Extensions.Logging;

namespace FacturamaNetSDK.Client;

/// <summary>
/// Cliente principal del SDK de Facturama.
/// </summary>
public sealed class FacturamaClient
{
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
    /// Operaciones de catálogos (/catalogs).
    /// </summary>  
    public ICatalogEndpoint Catalogs { get; }

    public IRetentionEndpoint Retentions { get; }   

    // -------------------------------------------------------------------------
    // Constructores
    // -------------------------------------------------------------------------

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

        var webClient = FacturamaHttpClientFactory.CreateApiClient(options, "/3", logger);
        var liteVersioned = FacturamaHttpClientFactory.CreateApiClient(options, apiLitePrefix, logger);
        var liteClient = FacturamaHttpClientFactory.CreateApiClient(options, "api-lite", logger);
        var rootClient = FacturamaHttpClientFactory.CreateRootClient(options, logger);
        var retentionClient = FacturamaHttpClientFactory.CreateApiClient(options, "api", logger);

        Cfdi = new CfdiEndpoint(webClient);
        CfdiLite = new CfdiLiteEndpoint(liteVersioned, liteClient, rootClient, webClient);
        Clients = new ClientEndpoint(rootClient);
        Catalogs = new CatalogEndpoint(rootClient);
        Retentions = new RetentionEndpoint(retentionClient);


    }
}