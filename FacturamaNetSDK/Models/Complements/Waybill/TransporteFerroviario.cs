namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class TransporteFerroviario
    {
        public string TipoDeServicio { get; set; }
        public string TipoDeTrafico { get; set; }
        public string NombreAseg { get; set; }
        public string NumPolizaSeguro { get; set; }
        public string Concesionario { get; set; }
        public DerechosDePaso[] DerechosDePaso { get; set; }

        public Carro[] Carro { get; set; }
    }
}
