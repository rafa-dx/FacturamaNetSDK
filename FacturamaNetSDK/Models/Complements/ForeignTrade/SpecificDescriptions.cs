using System.Text.Json.Serialization;


namespace Facturama.Sdk.Core.Models.Complements.ForeignTrade
{
    public sealed class SpecificDescriptions
    {
        /// <summary>
        /// Marca de la Mercancia
        /// </summary>
        [JsonPropertyName("Brand")]
        public string Brand { get; set; }

        /// <summary>
        /// Modelo de la mercancía
        /// </summary>
        [JsonPropertyName("Model")]
        public string Model { get; set; }

        /// <summary>
        /// Submodelo de la mercancía
        /// </summary>
        [JsonPropertyName("Submodel")]
        public string SubModel { get; set; }

        /// <summary>
        /// Número de serie de la mercancía
        /// </summary>  
        [JsonPropertyName("SerialNumber")]
        public string SerialNumber { get; set; }
    }
}
