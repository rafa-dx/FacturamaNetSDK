namespace FacturamaNetSDK.Models.TaxEntity.Request
{
    public sealed record TaxEntityCsdRequest
    {
        public string Rfc { get; set; }    
        public string Certificate {  get; set; }
        public string PrivateKey { get; set; }
        public string PrivateKeyPassword { get; set; }

    }
}
