
using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.BranchOffice.Response
{
    public sealed record BranchOfficeResponse
    {

        public string Id { get; init; }


        public string Name { get; init; }


        public string Description { get; init; }


        public Address Address { get; init; }


        public bool IsDefault { get; init; }

    }
}
