using FacturamaNetSDK.Models.Retentions.Complements;

namespace FacturamaNetSDK.Models.Retentions.Request
{
    public sealed class ComplementoRequest
    {

		public ServiciosPlataformasTecnologicas ServiciosPlataformasTecnologicas { get; set; }
        public TimbreFiscalDigital TimbreFiscalDigital { get; set; }
        public Intereses Intereses { get; set; }
    }
}
