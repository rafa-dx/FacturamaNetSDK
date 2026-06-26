using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class Remolque
    {
        [JsonPropertyName("SubTipoRem")]
        public string SubTipoRem { get; set; }


        [JsonPropertyName("Placa")]
        public string Placa { get; set; }
    }
}
