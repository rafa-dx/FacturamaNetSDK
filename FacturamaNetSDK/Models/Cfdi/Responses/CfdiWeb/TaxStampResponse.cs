using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturamaNetSDK.Models.Cfdi.Responses.CfdiWeb
{
    public record TaxStampResponse
    {
        public string Uuid { get; init; }
        public string Date { get; init; }
        public string CfdiSign { get; init; }
        public string SatCertNumber { get; init; }
        public string SatSign { get; init; }
        public string RfcProvCertif { get; init; }
    }
}
