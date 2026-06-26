namespace FacturamaNetSDK.Models.TaxEntity.Response
{
    public sealed record TaxStampResponse
    {

        public string Uuid { get; init; }
        public string Date { get; init; }
        public string CfdiSign { get; init; }
        public string SatCertNumber { get; init; }
        public string SatSign { get; init; }

        public string RfcProvCertif {  get; init; }

        public string AutNumProvCertif { get; init; }
    }
}
