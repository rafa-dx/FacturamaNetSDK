using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.Cfdi.Requests
{
    public sealed class Receiver
    {
        public string? Id { get; set; }
        public string? Rfc { get; set; }
        public string? Name { get; set; }
        public string? CfdiUse { get; set; }  
        public string? FiscalRegime { get; set; }
        public string? TaxZipCode { get; set; }
        public string? TaxResidence { get; set; }
        public string? TaxRegistrationNumber { get; set; }
        public Address Address { get; set; }
    }
}
