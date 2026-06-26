using System.Text.Json.Serialization;


namespace Facturama.Sdk.Core.Models.Complements.ForeignTrade
{
    public sealed class Recipient
    {
        /// <summary>
        ///  Número de identificación o registro fiscal del país de residencia para efectos fiscales del destinatario de la mercancía exportada.
        /// </summary>
        [JsonPropertyName("NumRegIdTrib")]
        public string NumRegIdTrib { get; set; }

        /// <summary>
        /// Nombre completo, denominación o razón social del destinatario de la mercancía exportada.
        /// </summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Address")]
        public List<Address> Addresses { get; set; }
    }
}
