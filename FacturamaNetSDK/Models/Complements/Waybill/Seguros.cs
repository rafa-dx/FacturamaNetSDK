using System.Text.Json.Serialization;

namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class Seguros
    {
        [JsonPropertyName("AseguraRespCivil")]
        public string AseguraRespCivil { get; set; }

        [JsonPropertyName("PolizaRespCivil")]
        public string PolizaRespCivil { get; set; }

        [JsonPropertyName("AseguraMedAmbiente")]
        public string AseguraMedAmbiente { get; set; }

        [JsonPropertyName("PolizaMedAmbiente")]
        public string PolizaMedAmbiente { get; set; }

        [JsonPropertyName("AseguraCarga")]
        public string AseguraCarga { get; set; }

        [JsonPropertyName("PolizaCarga")]
        public string PolizaCarga { get; set; }

        [JsonPropertyName("PrimaSeguro")]
        public decimal PrimaSeguro { get; set; }
    }
}
