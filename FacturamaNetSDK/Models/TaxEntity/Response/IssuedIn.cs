  
namespace FacturamaNetSDK.Models.TaxEntity.Response
{
    public sealed record  IssuedIn
    {
        public string Street { get; init; }
        public string Neighbour { get; init; }
        public string ZipCode { get; init; } 

        public string Municipality { get; init; }
        public string State { get; init; }
        public string Country { get; init; }
        public string Id { get; init; }
    }
}
