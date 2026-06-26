using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.Client.Request
{
    public class ClientRequest
    {

        public string Id { get; set; }
        public string? Email { get; set; }
        public string? EmailOp1 { get; set; }
        public Address Address { get; set; }
        public string Rfc { get; set; }
        public string Name { get; set; }
        public string CfdiUse { get; set; }
        public string FiscalRegime { get; set; }
        public string TaxZipCode { get; set; }
        public string? TaxResidence { get; set; }
        public string? NumRegIdTrib { get; set; }
    }
}
