using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.TaxEntity.Request
{
    public sealed class TaxEntityRequest
    {  
        public string FiscalRegime { get; set; }        
        public string ComercialName { get; set; }        
        public string Rfc { get; set; }       
        public string TaxName { get; set; }       
        public string Email { get; set; }       

        public string OptionalEmail { get; set; }
        public string Phone { get; set; }      
        public Address TaxAddress { get; set; } 
        // string PasswordSat { get; set; }
    }
}
