
using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class DocumentacionAduanera
    {
        [JsonPropertyName("TipoDocumento")]
        public string TipoDocumento { get; set; }

        [JsonPropertyName("NumPedimento")]
        public string NumPedimento { get; set; }

        [JsonPropertyName("IdentDocAduanero")]
        public string IdentDocAduanero { get; set; }

        [JsonPropertyName("RFCImpo")]
        public string RFCImpo { get; set; }
    }
}
