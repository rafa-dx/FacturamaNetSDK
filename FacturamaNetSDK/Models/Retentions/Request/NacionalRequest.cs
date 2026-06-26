using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturamaNetSDK.Models.Retentions.Request
{
    public sealed class NacionalRequest
    {
	
        public string RFCRecep { get; set; }
        public string NomDenRazSocR { get; set; }
        public string CurpR { get; set; }
        public string DomicilioFiscalR { get; set; }
    }
}
