using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.Complements.Payments
{
    public sealed class Payment
    {
  
        public string Date { get; set; }
        public string PaymentForm { get; set; }
        public string Currency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal Amount { get; set; }
        public string OperationNumber { get; set; }
        public string RfcIssuerPayerAccount { get; set; }
        public string ForeignAccountNamePayer { get; set; }
        public string PayerAccount { get; set; }
        public string RfcReceiverBeneficiaryAccount { get; set; }
        public string BeneficiaryAccount { get; set; }
        public List<RelatedDocument> RelatedDocuments { get; set; }
        public List<Tax> Taxes { get; set; }
    }

      
    
}
