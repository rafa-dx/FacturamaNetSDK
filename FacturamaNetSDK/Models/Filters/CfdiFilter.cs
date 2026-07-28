
using FacturamaNetSDK.Enums;

namespace FacturamaNetSDK.Models.Filters
{
    public sealed record CfdiFilter
    {
        public string keyword { get; init; }
        public string Type { get; init; }
        public string Status { get; init; } = InvoiceStatus.Active.ToString();
        public uint Page { get; init; } = uint.MinValue;
        public int FolioStart { get; init; } = -1;
        public int FolioEnd { get; init; } = -1;
        public string Rfc { get; init; }
        public string TaxEntityName { get; init; }
        public string DateStart { get; init; }
        public string DateEnd { get; init; } 
        public string IdBranch { get; init; } 
        public string Serie { get; init; } 
        public string Id { get; init; } 
        public string InvoiceType { get; init; } 
        public string PaymentMethod { get; init; }
        public string RfcIssuer { get; init; }
        public string OrderNumber { get; init; }
        //public string Folio { get; init; }
        //public string Uuid { get; init; }
        //public string RfcReceipt { get; init; }


    }
}