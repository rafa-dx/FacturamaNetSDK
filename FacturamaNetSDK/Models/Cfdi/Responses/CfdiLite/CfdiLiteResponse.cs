using FacturamaNetSDK.Models.Cfdi.Responses.Common;

namespace FacturamaNetSDK.Models.Cfdi.Responses.CfdiLite
{
    public record CfdiLiteResponse
    {
        public string Id { get; init; }
        public string CfdiType { get; init; }
        public string Type { get; init; }
        public string Serie { get; init; }
        public string Folio { get; init; }
        public string Date { get; init; }
        public string CertNumber { get; init; }
        public string PaymentTerms { get; init; }
        public string PaymentConditions { get; init; }
        public string PaymentMethod { get; init; }
        public string ExpeditionPlace { get; init; }
        public decimal ExchangeRate { get; init; }
        public string Currency { get; init; }
        public decimal Subtotal { get; init; }
        public decimal Discount { get; init; }
        public decimal Total { get; init; }
        public string Observations { get; init; }

        public IssuerResponse Issuer { get; init; }
        public ReceiverResponse Receiver { get; init; }
        public List<ItemResponse> Items { get; init; }
        public List<TaxResponse> Taxes { get; init; }
        public ComplementResponse? Complement { get; init; }

        public string? XmlBase64 { get; init; }

        public string? Status { get; init; }
        public string? OriginalString { get; init; }

        public TaxStampResponse TaxStamp { get; init; }

    }
}
