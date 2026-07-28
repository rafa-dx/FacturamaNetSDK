using FacturamaNetSDK.Models.Common;
using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Product.Response
{
    public sealed record ProductResponse
    {
        [JsonPropertyName("Id")]
        public string Id { get; init; }

        [JsonPropertyName("Unit")]
        public string Unit { get; init; }

        [JsonPropertyName("UnitCode")]
        public string UnitCode { get; init; }

        [JsonPropertyName("IdentificationNumber")]
        public string IdentificationNumber { get; init; }

        [JsonPropertyName("Category")]
        public string Category { get; init; }

        [JsonPropertyName("Name")]
        public string Name { get; init; }

        [JsonPropertyName("Description")]
        public string Description { get; init; }

        [JsonPropertyName("Price")]
        public decimal Price { get; init; }

        [JsonPropertyName("CodeProdServ")]
        public string CodeProdServ { get; init; }

        [JsonPropertyName("CuentaPredial")]
        public string CuentaPredial { get; init; }


        [JsonPropertyName("CuentasPredial")]
        public IReadOnlyCollection<string>? CuentasPredial { get; init; }

        [JsonPropertyName("ObjetoImp")]
        public string ObjetoImp { get; init; }

        [JsonPropertyName("Taxes")]
        public IEnumerable<Tax> Taxes { get; init; }
    }
}
