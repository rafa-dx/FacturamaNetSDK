

namespace FacturamaNetSDK.Models.Retentions.Request
{
    public sealed class TotalesRequest
    {

		public decimal montoTotOperacion { get; set; }
        public decimal montoTotGrav { get; set; }
        public decimal montoTotExent { get; set; }
        public decimal montoTotRet { get; set; }
        public List<ImpRetenido> ImpRetenidos { get; set; }
    }
}
