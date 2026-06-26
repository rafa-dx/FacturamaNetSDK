namespace FacturamaNetSDK.Models.Client.Request
{
    public sealed record CustomerValidationRequest
    {
        /// <summary>
        /// Rfc, consultado
        /// </summary>

        public string Rfc { get; set; }

        /// <summary>
        /// Name, Nombre Fiscal
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        /// ZipCode, Codigo postal 
        /// </summary>

        public string ZipCode { get; set; }

        /// <summary>
        /// FiscalRegime, Regimen Fiscal
        /// </summary>

        public string FiscalRegime { get; set; }
    }
}
