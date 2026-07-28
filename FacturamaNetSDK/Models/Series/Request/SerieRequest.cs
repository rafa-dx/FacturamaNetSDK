
namespace FacturamaNetSDK.Models.Series.Request
{
    public sealed record SerieRequest
    {
        public string IdBranchOffice { get; init; }

        public string Name { get; init; }

        public string Description { get; init; }

        public int Folio { get; init; }
    }
}
