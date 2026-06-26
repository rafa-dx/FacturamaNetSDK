using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facturama.Sdk.Core.Models.TaxEntity.Request
{
    public sealed record TaxEntityLogoRequest
    {
        public string Image {  get; set; }

        public string Type { get; set; }
    }
}
