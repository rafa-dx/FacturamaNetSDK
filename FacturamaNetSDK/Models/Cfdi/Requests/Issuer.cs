namespace FacturamaNetSDK.Models.Cfdi.Requests
{
    /// <summary>
    /// Datos fiscales del emisor del CFDI. En cuentas multiemisor (API Lite) se especifica por
    /// comprobante; en la API Web estándar el emisor se toma de la cuenta autenticada.
    /// </summary>
    public sealed class Issuer
    {
        /// <summary>Régimen fiscal del emisor — catálogo SAT c_RegimenFiscal. Obligatorio.</summary>
        public string FiscalRegime { get; set; } = string.Empty;

        /// <summary>RFC del emisor. Obligatorio.</summary>
        public string Rfc { get; set; } = string.Empty;

        /// <summary>Nombre o razón social del emisor. Obligatorio.</summary>
        public string Name { get; set; } = string.Empty;
    }
}
