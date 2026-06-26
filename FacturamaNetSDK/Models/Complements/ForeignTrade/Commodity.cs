using System.Text.Json.Serialization;


namespace Facturama.Sdk.Core.Models.Complements.ForeignTrade
{
    public sealed class Commodity
    {
        /// <summary>
        /// Número de parte, la clave de identificación que asigna la empresa o el número de serie de la mercancía exportada.
        /// </summary>
        [JsonPropertyName("IdentificationNumber")]
        public string IdentificationNumber { get; set; }

        /// <summary>
        /// Clave de la fracción arancelaria correspondiente a la descripción de la mercancía exportada.
        /// </summary>
        [JsonPropertyName("TariffFraction")]
        public string TariffFraction { get; set; }


        /// <summary>
        /// Cantidad de bienes en la aduana conforme a la UnidadAduana cuando en el nodo Comprobante:Conceptos:Concepto se hubiera registrado información comercial.
        /// </summary>
        [JsonPropertyName("CustomsQuantity")]
        public string CustomsQuantity { get; set; }

        /// <summary>
        /// Clave de la unidad de medida aplicable para la cantidad expresada en la mercancía en la aduana.
        /// </summary>
        [JsonPropertyName("CustomsUnit")]
        public string CustomsUnit { get; set; }

        /// <summary>
        /// Valor o precio unitario del bien en la aduana.
        /// </summary>
        [JsonPropertyName("CustomsUnitValue")]
        public decimal? CustomsUnitValue { get; set; }

        /// <summary>
        /// valor total en dólares (USD).
        /// </summary>
        [JsonPropertyName("ValueInDolar")]
        public decimal ValueInDolar { get; set; }

        /// <summary>
        /// lista de descripciones específicas de la mercancía.
        /// </summary>
        [JsonPropertyName("SpecificDescriptions")]
        public List<SpecificDescriptions> SpecificDescriptions { get; set; }
    }
}
