
using FacturamaNetSDK.Models.Common;
using FacturamaNetSDK.Models.Complements.ThirdPartyAccount;


namespace FacturamaNetSDK.Models.Cfdi.Requests
{
    public sealed class Item
    {
        
        public string? ProductId { get; set; } 
        public string ProductCode { get; set; }  
        public string? SKU { get; set; }
        public string? IdentificationNumber { get; set; }
        public string Description { get; set; }
        public string? Unit { get; set; }
        public string? UnitCode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; } 
        public decimal Subtotal { get; set; }   
        public string TaxObject { get; set; }
        public ThirdPartyAccountComplement ThirdPartyAccount { get; set; }
        public decimal? Discount { get; set; }
        public List<Tax> Taxes { get; set; }
        public List<string> PropertyTaxIDNumber { get; set; }
        public List<string> NumerosPedimento { get; set; }
        public decimal Total { get; set; }
        public ItemComplement? Complement { get; set; }
    }
}
