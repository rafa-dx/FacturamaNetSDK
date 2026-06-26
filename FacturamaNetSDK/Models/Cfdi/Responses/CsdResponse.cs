namespace FacturamaNetSDK.Models.Cfdi.Responses
{
    public sealed record CsdResponse
    {
        public string Rfc { get; init; }
        public string ExpirationDate { get; init; }
        public string Certificate {  get; init; }
        public string PrivateKey { get; init; }
        public string PrivateKeyPassword { get; init; }
    }
}
