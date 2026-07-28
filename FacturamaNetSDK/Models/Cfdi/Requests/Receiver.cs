using FacturamaNetSDK.Models.Common;

namespace FacturamaNetSDK.Models.Cfdi.Requests
{
    /// <summary>
    /// Datos fiscales del receptor del CFDI.
    /// </summary>
    public sealed class Receiver
    {
        /// <summary>Id del cliente registrado en Facturama (opcional; alternativa a capturar los datos).</summary>
        public string? Id { get; set; }

        /// <summary>RFC del receptor. Obligatorio. Ej. "URE180429TM6" o "XAXX010101000" (público en general).</summary>
        public string? Rfc { get; set; }

        /// <summary>Nombre o razón social tal como aparece en la Constancia de Situación Fiscal. Obligatorio.</summary>
        public string? Name { get; set; }

        /// <summary>Uso del CFDI — catálogo SAT c_UsoCFDI. Ej. "G03" (gastos en general).</summary>
        public string? CfdiUse { get; set; }

        /// <summary>Régimen fiscal del receptor — catálogo SAT c_RegimenFiscal. Ej. "601", "612".</summary>
        public string? FiscalRegime { get; set; }

        /// <summary>Código postal del domicilio fiscal del receptor. Obligatorio en CFDI 4.0.</summary>
        public string? TaxZipCode { get; set; }

        /// <summary>Residencia fiscal (país) para receptores extranjeros — catálogo SAT c_Pais (opcional).</summary>
        public string? TaxResidence { get; set; }

        /// <summary>Número de registro de identidad fiscal para extranjeros (opcional).</summary>
        public string? TaxRegistrationNumber { get; set; }

        /// <summary>Domicilio del receptor (opcional).</summary>
        public Address? Address { get; set; }
    }
}
