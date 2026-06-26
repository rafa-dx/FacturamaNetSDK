namespace FacturamaNetSDK.Models.Complements.Payroll
{
    public sealed class Employee
    {

        public Outsourcing[] Outsourcing { get; set; }


        public string Curp { get; set; }

        public string SocialSecurityNumber { get; set; }


        public DateTime? StartDateLaborRelations { get; set; }


        public string ContractType { get; set; }


        public bool Unionized { get; set; }


        public string TypeOfJourney { get; set; }


        public string RegimeType { get; set; }


        public string EmployeeNumber { get; set; }


        public string Department { get; set; }

 
        public string Position { get; set; }


        public string PositionRisk { get; set; }


        public string FrequencyPayment { get; set; }


        public string Bank { get; set; }


        public string BankAccount { get; set; }

        public decimal BaseSalary { get; set; }


        public decimal? DailySalary { get; set; }


        public string FederalEntityKey { get; set; }
    }
}
