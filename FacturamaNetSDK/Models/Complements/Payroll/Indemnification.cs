namespace FacturamaNetSDK.Models.Complements.Payroll
{
    public sealed class Indemnification
    {

        public decimal TotalPaid { get; set; }


        public int YearsOfService { get; set; }


        public decimal LastMonthlySalaryOrd { get; set; }


        public decimal AccumulatedIncome { get; set; }


        public decimal NonAccumulatedIncome { get; set; }
    }
}
