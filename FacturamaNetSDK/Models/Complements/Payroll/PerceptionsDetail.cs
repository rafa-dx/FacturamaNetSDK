namespace FacturamaNetSDK.Models.Complements.Payroll
{
    public sealed class PerceptionsDetail
    {

        public ActionsOrTitles ActionsOrTitles { get; set; }


        public ExtraHour[] ExtraHours { get; set; }


        public string PerceptionType { get; set; }


        public string Code { get; set; }

        public string Description { get; set; }

        public decimal TaxedAmount { get; set; }

        public decimal ExemptAmount { get; set; }
    }
}
