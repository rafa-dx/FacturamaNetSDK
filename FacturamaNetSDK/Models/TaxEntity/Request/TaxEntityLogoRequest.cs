using System;

namespace FacturamaNetSDK.Models.TaxEntity.Request
{
    public sealed record TaxEntityLogoRequest
    {
        public string Image {  get; set; }

        public string Type { get; set; }
    }
}
