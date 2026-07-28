namespace FacturamaNetSDK.Models.Cfdi.Responses
{
    public sealed record CfdiListResponse
    {
        public string Id { get; init; }

        public string CfdiType { get; init; }
        public string Type { get; init; }
        public string Folio { get; init; }
        public string Serie { get; init; }
        public string TaxName { get; init; }
        public string Rfc { get; init; }
        public string RfcIssuer { get; init; }
        public string Date { get; init; }
        public decimal Subtotal { get; init; }
        public decimal Total { get; init; }
        public string Uuid { get; init; }
        public bool IsActive { get; init; }
        public string PaymentMethod { get; init; }
        public string OrderNumber { get; init; }
        public string Status { get; init; }
        public bool EmailSent { get; init; }
    }
}
