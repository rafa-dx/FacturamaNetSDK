using System;

namespace FacturamaNetSDK.Models.Cfdi.Responses.CfdiWeb
{
    public record ReceiverResponse
    {
        public string? Rfc { get; set; }
        public string? Name { get; set; }

        public string? Email { get; set; }

    }
}
