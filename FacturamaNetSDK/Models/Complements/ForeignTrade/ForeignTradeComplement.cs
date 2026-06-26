using System.Text.Json.Serialization;


namespace Facturama.Sdk.Core.Models.Complements.ForeignTrade
{
    public sealed class ForeignTradeComplement
    {
        /// <summary>
        /// Emisor del complemento de comercio exterior
        /// </summary>
        [JsonPropertyName("Issuer")]
        public Issuer Issuer { get; set; }

        /// <summary>
        /// Receptor del complemento de comercio exterior
        /// </summary>
        [JsonPropertyName("Receiver")]
        public Receiver Receiver { get; set; }


        /// <summary>
        /// Datos del o los propietarios de la mecancía que se tarslada
        /// </summary>
        [JsonPropertyName("Owner")]
        public List<Owner> Owner { get; set; }

        /// <summary>
        /// Destinatario, datos complementarios del destinatario.
        /// </summary>
        [JsonPropertyName("Recipient")]
        public List<Recipient> Recipient { get; set; }

        /// <summary>
        /// Motivo Traslado
        /// </summary>
        [JsonPropertyName("ReasonForTrasnfer")]
        public string ReasonForTrasnfer { get; set; }


        /// <summary>
        /// Informacion de las mercancias que se trasladan
        /// </summary>
        [JsonPropertyName("Commodity")]
        public List<Commodity> Commodity { get; set; }


        /// <summary>
        /// clave de pedimento que se haya declarado conforme al catálogo
        /// </summary>
        [JsonPropertyName("RequestCode")]
        public string RequestCode { get; set; }


        /// <summary>
        /// Excepción de certificados de Origen de los Tratados de Libre Comercio.
        [JsonPropertyName("OriginCertificate")]
        public bool? OriginCertificate { get; set; }


        /// <summary>
        /// Folio del certificado de origen o el folio fiscal del CFDI con el que se pagó la expedición del certificado de origen.
        /// </summary>
        [JsonPropertyName("OriginCertificateNumber")]
        public string OriginCertificateNumber { get; set; }


        /// <summary>
        /// Número Exportador Confiable.
        /// </summary>
        [JsonPropertyName("ReliableExporterNumber")]
        public string ReliableExporterNumber { get; set; }


        /// <summary>
        ///  clave del INCOTERM aplicable a la factura.
        /// </summary>
        [JsonPropertyName("Incoterm")]
        public string Incoterm { get; set; }

        /// <summary>
        /// Información adicional.
        /// </summary>
        [JsonPropertyName("Observations")]
        public string Observations { get; set; }

        /// <summary>
        /// Tipo de Cambio USD, valor en pesos mexicanos que equivalen a un dólar de Estados Unidos de América
        /// </summary>
        [JsonPropertyName("ExchangeRateUSD")]
        public decimal? ExchangeRateUSD { get; set; }


        /// <summary>
        /// Total USD, importe total en dolares (USD)
        /// </summary>
        [JsonPropertyName("TotalUSD")]
        public decimal? TotalUSD { get; set; }
    }
}
