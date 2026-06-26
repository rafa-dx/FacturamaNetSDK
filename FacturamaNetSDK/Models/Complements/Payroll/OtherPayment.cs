namespace FacturamaNetSDK.Models.Complements.Payroll
{
    public sealed class OtherPayment
    {

        public EmploymentSubsidy EmploymentSubsidy { get; set; }


        public Compensation Compensation { get; set; }


        public string OtherPaymentType { get; set; }


        public string Code { get; set; }


        public string Description { get; set; }

        public decimal Amount { get; set; }
    }
}
