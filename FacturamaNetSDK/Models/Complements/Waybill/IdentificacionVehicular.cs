using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class IdentificacionVehicular
    {
        [JsonPropertyName("ConfigVehicular")]
        public string ConfigVehicular { get; set; }
        [JsonPropertyName("PesoBrutoVehicular")]
        public string PesoBrutoVehicular { get; set; }

        [JsonPropertyName("PlacaVM")]
        public string PlacaVM { get; set; }

        [JsonPropertyName("AnioModeloVM")]
        public int AnioModeloVM { get; set; }
    }
}
