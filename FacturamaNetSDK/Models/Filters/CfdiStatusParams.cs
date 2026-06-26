

namespace FacturamaNetSDK.Models.Filters
{
    public sealed record CfdiStatusParams
    {

        public string Uuid { get; init; }
        public string IssuerRfc { get; init; }

        public string ReceiverRfc { get; init; }
        public string Total { get; init; }

    }
}
