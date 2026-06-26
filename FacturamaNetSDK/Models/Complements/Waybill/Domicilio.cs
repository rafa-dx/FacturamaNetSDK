using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class Domicilio
    {
        [JsonPropertyName("Calle")]
        public string Calle { get; set; }

        [JsonPropertyName("NumeroExterior")]
        public string NumeroExterior { get; set; }

        [JsonPropertyName("NumeroInterior")]
        public string NumeroInterior { get; set; }

        [JsonPropertyName("Colonia")]
        public string Colonia { get; set; }

        [JsonPropertyName("Localidad")]
        public string Localidad { get; set; }

        [JsonPropertyName("Referencia")]
        public string Referencia { get; set; }

        [JsonPropertyName("Municipio")]
        public string Municipio { get; set; }

        [JsonPropertyName("Estado")]
        public string Estado { get; set; }

        [JsonPropertyName("Pais")]
        public string Pais { get; set; }

        [JsonPropertyName("CodigoPostal")]
        public string CodigoPostal { get; set; }
    }
}
