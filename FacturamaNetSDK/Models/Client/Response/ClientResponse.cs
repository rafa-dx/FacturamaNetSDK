using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.Client.Response
{
    public sealed class ClientResponse
    {

        public string Id { get; init; }
        public string Email { get; init; }
        public string EmailOp1 { get; init; }
        public string EmailOp2 { get; init; }
        public Address Address { get; init; }
        public string Rfc { get; init; }
        public string Name { get; init; }
        public string CfdiUse { get; init; }

        public string FiscalRegime { get; init; }
        public string TaxZipCode { get; init; }
        public string TaxResidence { get; init; }
        public string NumRegIdTrib { get; init; }
    }
}
