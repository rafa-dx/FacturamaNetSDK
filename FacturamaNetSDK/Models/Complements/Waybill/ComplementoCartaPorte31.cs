using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public enum RegistroISTMO
    {
        [EnumMember(Value = "Sí")]
        Sí
    }
    public enum TranspInternac
    {
        [EnumMember(Value = "Si")]
        Si,

        [EnumMember(Value = "No")]
        No,
    }
    public enum TipoUbicacion
    {
        [EnumMember(Value = "Origen")]
        Origen,

        [EnumMember(Value = "Destino")]
        Destino,
    }
    public sealed class ComplementoCartaPorte31
    {
        [JsonPropertyName("IdCCP")]
        public string IdCCP { get; set; }

        [JsonPropertyName("TranspInternac")]
        public TranspInternac TranspInternac { get; set; }

        [JsonPropertyName("RegimensAduaneros")]
        public RegimenAduaneroCPP[] RegimenesAduaneros { get; set; }

        [JsonPropertyName("EntradaSalidaMerc")]
        public string EntradaSalidaMerc { get; set; }

        [JsonPropertyName("PaisOrigenDestino")]
        public string PaisOrigenDestino { get; set; }

        [JsonPropertyName("ViaEntradaSalida")]
        public string ViaEntradaSalida { get; set; }

        [JsonPropertyName("TotalDistRec")]
        public decimal? TotalDistRec { get; set; }

        [JsonPropertyName("RegistroISTMO")]
        public RegistroISTMO? RegistroISTMO { get; set; }

        [JsonPropertyName("UbicacionPoloOrigen")]
        public string UbicacionPoloOrigen { get; set; }

        [JsonPropertyName("UbicacionPoloDestino")]
        public string UbicacionPoloDestino { get; set; }

        [JsonPropertyName("Ubicaciones")]
        public Ubicacion[] Ubicaciones { get; set; }

        [JsonPropertyName("Mercancias")]
        public Mercancias Mercancias { get; set; }

        [JsonPropertyName("FiguraTransporte")]
        public TiposFigura[] FiguraTransporte { get; set; }
    }
}
