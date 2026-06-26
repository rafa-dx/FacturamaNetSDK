namespace FacturamaNetSDK.Models.Complements.Payroll
{
    public sealed class PayrollComplement
    {

        public Issuer Issuer { get; set; }


        public Employee Employee { get; set; }

   
        public Perceptions Perceptions { get; set; }

 
        public Deductions Deductions { get; set; }

   
        public OtherPayment[] OtherPayments { get; set; }


        public Incapacity[] Incapacities { get; set; }

  
        public string Type { get; set; }


        public DateTime PaymentDate { get; set; }


        public DateTime setialPaymentDate { get; set; }


        public DateTime FinalPaymentDate { get; set; }


        public decimal DaysPaid { get; set; }


        public decimal DailySalary { get; set; }


        public decimal BaseSalary { get; set; }
    }
}
