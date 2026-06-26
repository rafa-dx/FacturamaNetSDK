namespace FacturamaNetSDK.Models.Retentions.Request
{
    public sealed class ImpRetenido
    {	
		public decimal? BaseRet { get; set; }
        public string Impuesto { get; set; }
        public decimal MontoRet { get; set; }
        public string TipoPagoRet { get; set; }
    }
}
