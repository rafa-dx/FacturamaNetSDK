
using FacturamaNetSDK.Models.Complements.ThirdPartyAccount;
using FacturamaNetSDK.Models.Complements.EducationalInstitution;

namespace FacturamaNetSDK.Models.Cfdi.Requests
{
    public class ItemComplement
    {

        public EducationalInstitutionComplement? EducationalInstitution { get; set; }

        public ThirdPartyAccountComplement? ThirdPartyAccount { get; set; }
    }
}
