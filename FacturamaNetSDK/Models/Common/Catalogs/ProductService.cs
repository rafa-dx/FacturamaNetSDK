namespace FacturamaNetSDK.Models.Common.Catalogs
{
    public sealed class ProductService : CatalogBase
    {
        public string IncludeIva { get; set; }
        public string IncludeIeps { get; set; }
        public string Complement { get; set; }

        public string DangerousMaterial { get; set; }
    }
}
