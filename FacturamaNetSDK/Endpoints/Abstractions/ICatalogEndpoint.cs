
using FacturamaNetSDK.Models.Common.Catalogs;

namespace FacturamaNetSDK.Endpoints.Abstractions;

/// <summary>
/// Catálogos del SAT disponibles en la API de Facturama.
/// </summary>
public interface ICatalogEndpoint
{
    /// <summary>Busca códigos postales por keyword.</summary>
    Task<IReadOnlyList<PostalCode>> GetPostalCodesAsync(
        string keyword, 
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene los tipos de relación entre CFDIs.</summary>
    Task<IReadOnlyList<RelationType>> GetRelationTypesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene el catálogo de países.</summary>
    Task<IReadOnlyList<Country>> GetCountriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene estados por código de país.</summary>
    Task<IReadOnlyList<State>> GetStatesAsync(
        string countryCode, 
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene municipios por código de estado.</summary>
    Task<IReadOnlyList<Municipality>> GetMunicipalitiesAsync(
        string? stateCode, 
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene localidades por código de municipio.</summary>
    Task<IReadOnlyList<Locality>> GetLocalitiesAsync(
        string? municipalityCode, 
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene colonias por código de localidad.</summary>
    Task<IReadOnlyList<Neighborhood>> GetNeighborhoodsAsync(
        string localityCode, 
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene los usos de CFDI disponibles.</summary>
    Task<IReadOnlyList<CfdiUse>> GetCfdiUsesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Busca unidades de medida por keyword.</summary>
    Task<IReadOnlyList<Unit>> GetUnitsAsync(
        string? keyword = null, 
        CancellationToken cancellationToken = default);

    /// <summary>Busca productos y servicios del SAT por keyword.</summary>
    Task<IReadOnlyList<ProductService>> GetProductsServicesAsync(
        string? keyword = null, 
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene los Name IDs disponibles.</summary>
    Task<IReadOnlyList<NameId>> GetNameIdsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Busca monedas por keyword.</summary>
    Task<IReadOnlyList<Currency>> GetCurrenciesAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene el catálogo de bancos.</summary>
    Task<IReadOnlyList<Bank>> GetBanksAsync(
        
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene las formas de pago disponibles.</summary>
    Task<IReadOnlyList<PaymentForm>> GetPaymentFormsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene los métodos de pago disponibles.</summary>
    Task<IReadOnlyList<PaymentMethod>> GetPaymentMethodsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene los tipos de CFDI disponibles.</summary>
    Task<IReadOnlyList<CfdiType>> GetCfdiTypesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Busca regímenes fiscales por RFC.</summary>
    Task<IReadOnlyList<FiscalRegimen>> GetFiscalRegimensAsync(
        string? rfc = null,
        CancellationToken cancellationToken = default);


    /// <summary>Obtiene los incoterms disponibles.</summary>
    Task<IReadOnlyList<Incoterm>> GetIncotermAsync(
        CancellationToken cancellationToken = default);

    ///
    Task<IReadOnlyList<TariffFractions>> GetTariffFractionsAsync(
        string keyword,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaveUnidadPeso>> GetClaveUnidadPesoAsync(
        CancellationToken cancellationToken= default);

    Task<IReadOnlyList<CatalogTransportKey>> GetCatalogTransportKeyAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CondicionesEspeciales>> GetCondicionesEspecialesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConfigAutotransporte>> GetConfigAutotransporteAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoAduanero>> GetDocumentoAduaneroAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FormaFarmaceutica>> GetFormaFarmaceuticaAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TipoEmbalaje>> GetTipoEmbalajeAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubTipoRemolque>> GetSubTipoRemolqueAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MaterialPeligroso>> GetMaterialPeligrosoAsync(
        string keyword, 
        CancellationToken cancellationToken = default);
        
    
    Task<IReadOnlyList<TipoMateria>> GetTipoMateriaAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TipoPermiso>> GetTipoPermisoAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegimenAduanero>> GetRegimenAduaneroEntradaAsync(
        CancellationToken cancellationToken = default);


    Task<IReadOnlyList<RegimenAduanero>> GetRegimenAduaneroSalidaAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegistroISTMO>> GetRegistroISTMOAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SectorCOFEPRIS>> GetSectorCOFEPRISAsync(
        CancellationToken cancellationToken = default);
        
    Task<IReadOnlyList<Mercancias>> GetMercanciasAsync(
            CancellationToken cancellationToken = default);

}