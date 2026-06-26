using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class Pedimentos
    {
        [JsonPropertyName("Pedimento")]
        public string Pedimento { get; set; }
    }
}
