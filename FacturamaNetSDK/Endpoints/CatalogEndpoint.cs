using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Common.Catalogs;


namespace FacturamaNetSDK.Endpoints;

/// <summary>
/// Catálogos del SAT (/Catalogs).
/// </summary>
public sealed class CatalogEndpoint : ICatalogEndpoint
{
    private const string Resource = "Catalogs";
    private const string SubResource = "cartaporte";
    private readonly FacturamaHttpClient _client;

    internal CatalogEndpoint(FacturamaHttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    // -------------------------------------------------------------------------
    // Helper interno — evita repetición en cada método
    // -------------------------------------------------------------------------

    private Task<IReadOnlyList<T>> GetCatalogAsync<T>(
        string resource,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
        => _client.GetAsync<IReadOnlyList<T>>($"{Resource}/{resource}", queryParams, cancellationToken);

    private static Dictionary<string, string?> KeywordParam(string keyword)
        => new() { ["keyword"] = keyword };

    // -------------------------------------------------------------------------
    // Métodos públicos
    // -------------------------------------------------------------------------

    public Task<IReadOnlyList<PostalCode>> GetPostalCodesAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(keyword))
            throw new ArgumentException("El parámetro es obligatorio.", nameof(keyword));

        return GetCatalogAsync<PostalCode>(
            "postalcodes", 
            KeywordParam(keyword), 
            cancellationToken);
    }

    public Task<IReadOnlyList<RelationType>> GetRelationTypesAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<RelationType>("relationtypes", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<Country>> GetCountriesAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<Country>("countries", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<State>> GetStatesAsync(
        string countryCode,
        CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("El parámetro es obligatorio.", nameof(countryCode));

        return GetCatalogAsync<State>("states",
            new() { ["countryCode"] = countryCode },
            cancellationToken);
    }

    public Task<IReadOnlyList<Municipality>> GetMunicipalitiesAsync(
        string? stateCode,
        CancellationToken cancellationToken = default)
    {

        return GetCatalogAsync<Municipality>("municipalities",
            stateCode != null ? new() { ["stateCode"] = stateCode } : null,
            cancellationToken);
    }

    public Task<IReadOnlyList<Locality>> GetLocalitiesAsync(
        string? municipalityCode,
        CancellationToken cancellationToken = default)
    {

        return GetCatalogAsync<Locality>("localities",
            municipalityCode != null ? new() { ["municipalityCode"] = municipalityCode } : null,
            cancellationToken);
    }

    public Task<IReadOnlyList<Neighborhood>> GetNeighborhoodsAsync(
        string localityCode,
        CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(localityCode))
            throw new ArgumentException("El parámetro es obligatorio.", nameof(localityCode));

        return GetCatalogAsync<Neighborhood>("neighborhoods",
            new() { ["postalCode"] = localityCode },
            cancellationToken);
    }

    public Task<IReadOnlyList<CfdiUse>> GetCfdiUsesAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<CfdiUse  >("cfdiuses", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<Unit  >> GetUnitsAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<Unit>("Units",
            keyword is not null ? KeywordParam(keyword) : null,
            cancellationToken);

    public Task<IReadOnlyList<ProductService>> GetProductsServicesAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<ProductService>("productsorservices",
            keyword is not null ? KeywordParam(keyword) : null,
            cancellationToken);

    public Task<IReadOnlyList<NameId>> GetNameIdsAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<NameId>("nameids", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<Currency>> GetCurrenciesAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<Currency>("currencies",
            keyword is not null ? KeywordParam(keyword) : null,
            cancellationToken);

    public Task<IReadOnlyList<Bank>> GetBanksAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<Bank>("banks", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<PaymentForm>> GetPaymentFormsAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<PaymentForm>("paymentforms", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<PaymentMethod>> GetPaymentMethodsAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<PaymentMethod>("paymentmethods", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<CfdiType>> GetCfdiTypesAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<CfdiType>("cfditypes", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<FiscalRegimen>> GetFiscalRegimensAsync(
        string? rfc = null,
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<FiscalRegimen>("fiscalregimens",
            rfc is not null ? new() { ["rfc"] = rfc } : null,
            cancellationToken);


    public Task<IReadOnlyList<TariffFractions>> GetTariffFractionsAsync(
        string keyword,
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<TariffFractions>("tarifffractions",
            KeywordParam(keyword),
            cancellationToken);
    public Task<IReadOnlyList<Incoterm>> GetIncotermAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<Incoterm>("Incoterm", 
            cancellationToken: cancellationToken);
    public Task<IReadOnlyList<ClaveUnidadPeso>> GetClaveUnidadPesoAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<ClaveUnidadPeso>($"{SubResource}/ClaveUnidadPeso", 
            cancellationToken: cancellationToken);

    public Task<IReadOnlyList<CatalogTransportKey>> GetCatalogTransportKeyAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<CatalogTransportKey>($"{SubResource}/CatalogTransportKey", 
            cancellationToken: cancellationToken);

    public Task<IReadOnlyList<CondicionesEspeciales>> GetCondicionesEspecialesAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<CondicionesEspeciales>($"{SubResource}/CondicionesEspeciales", 
            cancellationToken: cancellationToken);

    public Task<IReadOnlyList<ConfigAutotransporte>> GetConfigAutotransporteAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<ConfigAutotransporte>($"{SubResource}/ConfigAutotransporte", 
            cancellationToken: cancellationToken);

    public Task<IReadOnlyList<DocumentoAduanero>> GetDocumentoAduaneroAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<DocumentoAduanero>($"{SubResource}/DocumentoAduanero", 
            cancellationToken: cancellationToken);
    public Task<IReadOnlyList<FormaFarmaceutica>> GetFormaFarmaceuticaAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<FormaFarmaceutica>($"{SubResource}/FormaFarmaceutica", 
            cancellationToken: cancellationToken);
    public Task<IReadOnlyList<TipoEmbalaje>> GetTipoEmbalajeAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<TipoEmbalaje>($"{SubResource}/TipoEmbalaje", 
            cancellationToken: cancellationToken);

    public Task<IReadOnlyList<SubTipoRemolque>> GetSubTipoRemolqueAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<SubTipoRemolque>($"{SubResource}/SubTipoRemolque", 
            cancellationToken: cancellationToken);

    public Task<IReadOnlyList<MaterialPeligroso>> GetMaterialPeligrosoAsync(
        string keyword,
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<MaterialPeligroso>($"{SubResource}/MaterialPeligroso",
            KeywordParam(keyword),
            cancellationToken: cancellationToken);

    public Task<IReadOnlyList<TipoMateria>> GetTipoMateriaAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<TipoMateria>($"{SubResource}/TipoMateria", 
            cancellationToken: cancellationToken);

    public Task<IReadOnlyList<TipoPermiso>> GetTipoPermisoAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<TipoPermiso>($"{SubResource}/TipoPermiso", 
            cancellationToken: cancellationToken);
    public Task<IReadOnlyList<RegimenAduanero>> GetRegimenAduaneroEntradaAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<RegimenAduanero>($"{SubResource}/RegimenAduanero/Entrada", 
            cancellationToken: cancellationToken);

    public Task<IReadOnlyList<RegimenAduanero>> GetRegimenAduaneroSalidaAsync(
     CancellationToken cancellationToken = default)
     => GetCatalogAsync<RegimenAduanero>($"{SubResource}/RegimenAduanero/Salida",
         cancellationToken: cancellationToken);
    public Task<IReadOnlyList<RegistroISTMO>> GetRegistroISTMOAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<RegistroISTMO>($"{SubResource}/RegistroISTMO",
            cancellationToken: cancellationToken);
    public Task<IReadOnlyList<SectorCOFEPRIS>> GetSectorCOFEPRISAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<SectorCOFEPRIS>($"{SubResource}/SectorCOFEPRIS",
            cancellationToken: cancellationToken);


    public Task<IReadOnlyList<Mercancias>> GetMercanciasAsync(
        CancellationToken cancellationToken = default)
        => GetCatalogAsync<Mercancias>($"{SubResource}/Mercancias",
            cancellationToken: cancellationToken);

}