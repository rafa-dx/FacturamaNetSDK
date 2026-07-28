
using FacturamaNetSDK.Enums;

namespace FacturamaNetSDK.Models.Filters
{
    public sealed record RetentionFilter
    {
        public string keyword { get; init; }

        public string status { get; init; } = InvoiceStatus.All.ToString().ToLower();
        public string Type { get; init; } = InvoiceType.Retention.ToString().ToLower();
        public int FolioStart { get; init; } = -1;
        public int FolioEnd { get; init; } = -1;
        public string Rfc { get; init; }
        public string RfcIssuer { get; init; }
        public string DateStart { get; init; }
        public string DateEnd { get; init; }
        public uint Page { get; init; } = uint.MinValue;
    }
}
