using FacturamaNetSDK.Models.Cfdi.Responses;
using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.TaxEntity.Response
{
    public sealed record TaxEntityResponse
    {
        public string FiscalRegime { get; init; }
        public string ComercialName { get; init; }

        public string Rfc { get; init; }

        public string TaxName { get; init; }

        public string Email { get; init; }

        public string OptionalEmail { get; init; }

        public string Phone { get; init; }

        public Address TaxAddress   { get; init; }

        public IssuedIn IssuedIn { get; init; }

        public CsdResponse  Csd { get; init; }

        public CsdResponse Fiel { get; init; }
        public string UrlLogo { get; init; }    


    }
}
