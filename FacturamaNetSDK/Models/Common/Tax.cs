

using System.Text.Json.Serialization;

namespace FacturamaNetSDK.Models.Common
{
    public sealed class Tax
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public decimal Total { get; set; }
        public string Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public decimal Base { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public decimal Rate { get; set; }
        public bool IsRetention { get; set; }
        public bool IsQuota { get; set; }
        public bool IsFederalTax { get; set; }

    }
}
