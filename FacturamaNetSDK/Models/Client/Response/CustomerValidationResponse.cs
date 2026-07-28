using System;

namespace FacturamaNetSDK.Models.Client.Response
{
    public class CustomerValidationResponse
    {
        public bool ExistRfc { get; set; }
        public bool MatchName { get; set; }
        public bool MatchZipCode { get; set; }
        public bool MatchFiscalRegime { get; set; }
        public bool IsValid { get; set; }



    }
}
