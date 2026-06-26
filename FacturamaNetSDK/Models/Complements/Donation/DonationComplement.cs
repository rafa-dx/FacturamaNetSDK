namespace FacturamaNetSDK.Models.Complements.Donation
{
    public sealed class DonationComplement
    {
        /// <summary>
        /// Fecha del oficio en que se informó la autorización
        /// para recibir donativos deducibles.
        /// </summary>

        public DateTime AuthorizationDate { get; set; }

        /// <summary>
        /// Número del oficio de autorización o renovación.
        /// </summary>

        public string AuthorizationNumber { get; set; } = default!;

        /// <summary>
        /// Leyenda que indica que el comprobante deriva de un donativo.
        /// </summary>

        public string Legend { get; set; } = default!;
    }
}
