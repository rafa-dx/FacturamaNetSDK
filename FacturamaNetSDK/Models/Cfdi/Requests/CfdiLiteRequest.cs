namespace FacturamaNetSDK.Models.Cfdi.Requests
{
    public sealed class CfdiLiteRequest : CfdiRequest
    {
        public string LogoUrl { get; set; }

        public Issuer Issuer { get; set; } = default!;
    }
}
