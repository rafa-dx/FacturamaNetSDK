using FacturamaNetSDK.Models.Cfdi.Responses.Common;

namespace FacturamaNetSDK.Models.Cfdi.Responses.CfdiWeb
{
    public record CfdiResponse
    {
        public string Id { get; set; }
        public string CfdiType { get; set; }
        public string Type { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public string Date { get; set; }
        public string CertNumber { get; set; }
        public string PaymentTerms { get; set; }
        public string PaymentConditions { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentAccountNumber { get; set; }
        public string PaymentBankName { get; set; }
        public string ExpeditionPlace { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Currency { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }

        public decimal Total { get; set; }
        public string Observations { get; set; }
        public string OrderNumber { get; set; }

        public IssuerResponse Issuer { get; init; }
        public ReceiverResponse Receiver { get; init; }
        public List<ItemResponse> Items { get; init; }
        public List<TaxResponse> Taxes { get; init; }
        public ComplementResponse? Complement { get; init; }

        public string? Status { get; init; } 
        public string? OriginalString { get; init; }

    }
    
}
