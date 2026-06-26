namespace FacturamaNetSDK.Models.Cfdi.Responses.CfdiLite
{
    public record TaxResponse
    {
        public decimal? Total { get; init; }
        public string? Name { get; init; }
        public decimal? Rate { get; init; }
        public string? Type { get; init; }

        public string? Factor { get; init; }
    }
}
