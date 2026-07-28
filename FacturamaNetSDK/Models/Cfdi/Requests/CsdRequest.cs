using System;

namespace FacturamaNetSDK.Models.Cfdi.Requests
{
    public sealed record CsdRequest
    {
            public string Rfc { get; set; }
            public string Certificate { get; set; }
            public string PrivateKey { get; set; }
            public string PrivateKeyPassword { get; set; }
        
    }
}
