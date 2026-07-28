namespace FacturamaNetSDK.Models.Series.Response
{
    public sealed record SerieResponse
    {
        public int Folio { get; init; }

        public string Name { get; init; }

        public string Description { get; init; }
    }
}
