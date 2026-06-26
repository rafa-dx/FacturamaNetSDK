
namespace FacturamaNetSDK.Models.Common.Catalogs
{
    public sealed class PostalCode : CatalogBase
    {

        public string StateCode { get; init; }
        public string MunicipalityCode { get; init; }
        public string LocationCode { get; init; }
    }
}
