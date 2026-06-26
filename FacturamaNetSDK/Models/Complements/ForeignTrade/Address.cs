using System.Text.Json.Serialization;


namespace Facturama.Sdk.Core.Models.Complements.ForeignTrade
{
    public sealed class Address
    {
        [JsonPropertyName("Street")]
        public string Street { get; set; }

        [JsonPropertyName("ExteriorNumber")]
        public string ExteriorNumber { get; set; }

        [JsonPropertyName("InteriorNumber")]
        public string InteriorNumber { get; set; }

        [JsonPropertyName("Neighborhood")]
        public string Neighborhood { get; set; }

        [JsonPropertyName("Reference")]
        public string Reference { get; set; }

        [JsonPropertyName("Locality")]
        public string Locality { get; set; }

        [JsonPropertyName("Municipality")]
        public string Municipality { get; set; }

        [JsonPropertyName("State")]
        public string State { get; set; }

        [JsonPropertyName("Country")]
        public string Country { get; set; }

        [JsonPropertyName("ZipCode")]
        public string ZipCode { get; set; }
    }
}
