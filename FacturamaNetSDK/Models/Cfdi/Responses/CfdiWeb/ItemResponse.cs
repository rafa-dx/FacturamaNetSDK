using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturamaNetSDK.Models.Cfdi.Responses.CfdiWeb
{
    public record ItemResponse
    {
        public string? ProductCode { get; init; }
        public string? IdentificationNumber { get; init; }
        public string? UnitCode { get; init; }
        public decimal? Discount { get; init; }
        public string? CuentaPredial { get; init; }
        public decimal? Quantity { get; init; }
        public string? Unit { get; init; }
        public string? Description { get; init; }
        public decimal? UnitValue { get; init; }
        public decimal? Total { get; init; }
    }
}
