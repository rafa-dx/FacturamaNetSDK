using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.BranchOffice.Request
{
    public sealed record BranchOfficeRequest
    {

        public string Id { get; set; }


        public string Name { get; set; }


        public string Description { get; set; }

        public Address Address { get; set; }

    }
}
