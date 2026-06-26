namespace FacturamaNetSDK.Models.Complements.Payroll
{
    public sealed class Retirement
    {

        public decimal? TotalASinglePayment { get; set; }

        public decimal? TotalParciality { get; set; }


        public decimal? DailyAmount { get; set; }


        public decimal AccumulatedIncome { get; set; }

        public decimal NonAccumulatedIncome { get; set; }
    }
}
