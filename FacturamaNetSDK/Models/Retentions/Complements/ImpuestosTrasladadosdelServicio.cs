using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
