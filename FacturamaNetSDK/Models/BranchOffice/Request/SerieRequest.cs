
namespace FacturamaNetSDK.Models.BranchOffice.Request
{
    public sealed record SerieRequest
    {

        public string IdBranchOffice { get; set; }

        public string Name { get; set; }


        public string Description { get; set; }
        public long Folio { get; set; }
    }
}
