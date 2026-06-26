
using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.BranchOffice.Request
{
    public sealed record BranchOfficeResponse
    {

        public string Id { get; init; }


        public string Name { get; init; }


        public string Description { get; init; }


        public Address Address { get; init; }


        public string IsDefault { get; init; }

    }
}
