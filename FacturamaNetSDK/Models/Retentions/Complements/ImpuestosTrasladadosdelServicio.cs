using System;

namespace FacturamaNetSDK.Models.Retentions.Complements
{
    public sealed class ImpuestosTrasladadosdelServicio
    {
		public decimal Base { get; set; }
        public string Impuesto { get; set; }

        public string TipoFactor { get; set; }
        public decimal TasaCuota { get; set; }
        public decimal Importe { get; set; }
    }
}
