using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class RegimenAduaneroCPP
    {
        [JsonPropertyName("RegimenAduanero")]
        public string RegimenAduanero { get; set; }
    }
}
