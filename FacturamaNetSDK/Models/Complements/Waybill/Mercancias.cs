using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class Mercancias
    {

        [JsonPropertyName("PesoBrutoTotal")]
        public decimal PesoBrutoTotal { get; set; }

        [JsonPropertyName("UnidadPeso")]
        public string UnidadPeso { get; set; }


        [JsonPropertyName("PesoNetoTotal")]
        public decimal PesoNetoTotal { get; set; }

        [JsonPropertyName("NumTotalMercancias")]
        public int NumTotalMercancias { get; set; }

        [JsonPropertyName("CargoPorTasacion")]
        public decimal CargoPorTasacion { get; set; }

        [JsonPropertyName("LogisticaInversaRecoleccionDevolucion")]
        public string LogisticaInversaRecoleccionDevolucion { get; set; }


        [JsonPropertyName("Mercancia")]
        public Mercancia[] Mercancia { get; set; }

        [JsonPropertyName("Autotransporte")]
        public Autotransporte Autotransporte { get; set; }

        [JsonPropertyName("TransporteMaritimo")]
        public TransporteMaritimo TransporteMaritimo { get; set; }

        [JsonPropertyName("TransporteAereo")]
        public TransporteAereo TransporteAereo { get; set; }

        [JsonPropertyName("TransporteFerroviario")]
        public TransporteFerroviario TransporteFerroviario { get; set; }
    }
}
