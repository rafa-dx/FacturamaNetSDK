

namespace FacturamaNetSDK.Models.Cfdi.Responses.CfdiWeb
{
    public record IssuerResponse
    {
        public string FiscalRegime { get; init; }
        public string Rfc { get; init; }
        public string TaxName { get; init; }
        public string Email { get; init; }
        public string Phone { get; init; }
        public TaxAddressResponse TaxAddress { get; init; }

        public IssuedInResponse IssuedIn { get; init; }
    }

    public record TaxAddressResponse

    {
        public string? Street { get; init; }
        public string? ExteriorNumber { get; init; }
        public string? InteriorNumber { get; init; }
        public string? Neighborhood { get; init; }
        public string? ZipCode { get; init; }
        public string? Municipality { get; init; }
        public string? State { get; init; }
        public string? Country { get; init; }
    }

    public record IssuedInResponse
    {
        public string? Street { get; init; }
        public string? ExteriorNumber { get; init; }
        public string? InteriorNumber { get; init; }
        public string? Neighborhood { get; init; }
        public string? ZipCode { get; init; }
        public string? Municipality { get; init; }
        public string? State { get; init; }
        public string? Country { get; init; }
    }

}
