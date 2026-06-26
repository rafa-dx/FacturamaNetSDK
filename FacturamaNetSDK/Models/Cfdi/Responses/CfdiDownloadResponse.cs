using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Cfdi.Responses
{
    public class CfdiDownloadResponse
    {
        public string ContentEncoding { get; init; }
        public string ContentType { get; init; }
        public int ContentLength { get; init; }

        [JsonPropertyName("Content")]
        public string ContentBase64 { get; set; }

        [JsonIgnore]
        public byte[] ContentBytes => Convert.FromBase64String(ContentBase64);
    }
}
