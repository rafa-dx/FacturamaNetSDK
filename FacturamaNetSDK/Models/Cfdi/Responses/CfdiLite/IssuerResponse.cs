namespace FacturamaNetSDK.Models.Cfdi.Responses.CfdiLite
{
    public record IssuerResponse
    {
        public string FiscalRegime { get; init; }
        public string Rfc { get; init; }
        public string TaxName { get; init; }
        public string Phone { get; init; }
    }
}
