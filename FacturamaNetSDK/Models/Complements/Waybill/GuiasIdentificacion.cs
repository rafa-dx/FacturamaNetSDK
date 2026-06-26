using System.Text.Json.Serialization;

namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class GuiasIdentificacion
    {
        [JsonPropertyName("NumeroGuiaIdentificacion")]
        public string NumeroGuiaIdentificacion { get; set; }

        [JsonPropertyName("DescripGuiaIdentificacion")]
        public string DescripGuiaIdentificacion { get; set; }

        [JsonPropertyName("PesoGuiaIdentificacion")]
        public decimal PesoGuiaIdentificacion { get; set; }
    }
}
