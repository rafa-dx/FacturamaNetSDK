using FacturamaNetSDK.Models.Common;
using System.Text.Json.Serialization;


namespace Facturama.Sdk.Core.Models.Product.Request
{
    public sealed record ProductRequest
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("Unit")]
        public string Unit { get; set; }

        [JsonPropertyName("UnitCode")]
        public string UnitCode { get; set; }

        [JsonPropertyName("IdentificationNumber")]
        public string IdentificationNumber { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("Category")]
        public string Category { get; set; }

        [JsonPropertyName("Price")]
        public decimal Price { get; set; }

        [JsonPropertyName("CodeProdServ")]
        public string CodeProdServ { get; set; }

        [JsonPropertyName("CuentaPredial")]
        public string CuentaPredial { get; set; }

        [JsonPropertyName("CuentasPredial")]
        public IReadOnlyCollection<string>? CuentasPredial { get; set; }

        [JsonPropertyName("ObjetoImp")]
        public string ObjetoImp { get; set; }

        [JsonPropertyName("Taxes")]
        public IEnumerable<Tax> Taxes { get; set; }
    }
}
