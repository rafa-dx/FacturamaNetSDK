using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class DetalleMercancia
    {

        [JsonPropertyName("UnidadPesoMerc")]
        public string UnidadPesoMerc { get; set; }

        [JsonPropertyName("PesoBruto")]
        public decimal PesoBruto { get; set; }

        [JsonPropertyName("PesoNeto")]
        public decimal PesoNeto { get; set; }

        [JsonPropertyName("PesoTara")]
        public decimal PesoTara { get; set; }

        [JsonPropertyName("NumPiezas")]
        public int NumPiezas { get; set; }
    }
}
