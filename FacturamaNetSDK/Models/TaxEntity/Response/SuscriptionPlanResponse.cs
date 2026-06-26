namespace FacturamaNetSDK.Models.TaxEntity.Response
{
    public sealed record SuscriptionPlanResponse
    {
        public string Plan { get; init; }
        public string CurrentFolios { get; init; }
        public string CreationDate { get; init; }
        public string ExpirationDate { get; init; }    
    }
}
