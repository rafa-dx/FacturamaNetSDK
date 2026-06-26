

namespace FacturamaNetSDK.Models.Cfdi.Responses
{
    public class CfdiCancellationResponse
    {
        public string Status { get; init; }
        public string Message { get; init; }
        public string IsCancelable { get; init; }
        public string Uuid { get; init; }
        public string RequestDate { get; init; }
        public string ExpirationDate { get; init; }

        public string AcuseXmlBase64 { get; init; }
        public string CancelationDate { get; init; }
        public int AcuseStatus { get; init; }
        public string AcuseStatusDetails { get; init; }

    }
}
