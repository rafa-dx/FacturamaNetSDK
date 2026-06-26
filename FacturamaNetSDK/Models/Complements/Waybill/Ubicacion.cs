using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class Ubicacion
    {
        [JsonPropertyName("Domicilio")]
        public Domicilio Domicilio { get; set; }

        [JsonPropertyName("TipoUbicacion")]
        public TipoUbicacion TipoUbicacion { get; set; }

        [JsonPropertyName("IDUbicacion")]
        public string IDUbicacion { get; set; }

        [JsonPropertyName("RFCRemitenteDestinatario")]
        public string RFCRemitenteDestinatario { get; set; }

        [JsonPropertyName("NombreRemitenteDestinatario")]
        public string NombreRemitenteDestinatario { get; set; }

        [JsonPropertyName("NumRegIdTrib")]
        public string NumRegIdTrib { get; set; }

        [JsonPropertyName("ResidenciaFiscal")]
        public string ResidenciaFiscal { get; set; }

        [JsonPropertyName("NumEstacion")]
        public string NumEstacion { get; set; }

        [JsonPropertyName("NombreEstacion")]
        public string NombreEstacion { get; set; }

        [JsonPropertyName("NavegacionTrafico")]
        public string NavegacionTrafico { get; set; }

        [JsonPropertyName("FechaHoraSalidaLlegada")]
        public DateTime FechaHoraSalidaLlegada { get; set; }

        [JsonPropertyName("TipoEstacion")]
        public string TipoEstacion { get; set; }

        [JsonPropertyName("DistanciaRecorrida")]
        public decimal DistanciaRecorrida { get; set; }
    }
}
