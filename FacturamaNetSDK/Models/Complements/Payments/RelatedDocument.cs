using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.Complements.Payments
{
    public sealed class RelatedDocument
    {
        public string Uuid { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public string Currency { get; set; }
        public string EquivalenceDocRel { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string PaymentMethod { get; set; }
        public int PartialityNumber { get; set; }
        public decimal PreviousBalanceAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public string TaxObject { get; set; }

        public string ImpSaldoInsoluto { get; set; }

        public List<Tax> Taxes { get; set; }
    }
}
