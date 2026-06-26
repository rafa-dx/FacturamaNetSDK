using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class CantidadTransporta
    {
        [JsonPropertyName("Cantidad")]
        public decimal Cantidad { get; set; }

        [JsonPropertyName("IDOrigen")]
        public string IDOrigen { get; set; }

        [JsonPropertyName("IDDestino")]
        public string IDDestino { get; set; }

        [JsonPropertyName("CvesTransporte")]
        public string CvesTransporte { get; set; }
    }
}
