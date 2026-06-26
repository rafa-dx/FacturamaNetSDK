

using System.ComponentModel.DataAnnotations;

namespace Facturama.Sdk.Core.Models.Filters
{
    public sealed record CfdiCancellationParams
    {
        public string Type {  get; init; } = "issued";

        public string Motive { get; init; } = "02";

        public string uuidReplacement { get; init; } = string.Empty;


    }
}
