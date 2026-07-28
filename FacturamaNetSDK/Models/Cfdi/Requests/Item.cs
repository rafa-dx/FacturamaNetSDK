using FacturamaNetSDK.Models.Common;
using FacturamaNetSDK.Models.Complements.ThirdPartyAccount;

namespace FacturamaNetSDK.Models.Cfdi.Requests
{
    /// <summary>
    /// Concepto (partida) de un CFDI: el producto o servicio que se factura.
    /// </summary>
    public sealed class Item
    {
        /// <summary>Id del producto registrado en Facturama (opcional; alternativa a capturar los datos).</summary>
        public string? ProductId { get; set; }

        /// <summary>Clave de producto o servicio — catálogo SAT c_ClaveProdServ. Obligatorio. Ej. "10101504".</summary>
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>SKU o código interno del producto (opcional).</summary>
        public string? SKU { get; set; }

        /// <summary>Número de identificación / código de barras (opcional).</summary>
        public string? IdentificationNumber { get; set; }

        /// <summary>Descripción del concepto. Obligatorio.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Nombre de la unidad en texto libre (opcional). Ej. "Pieza".</summary>
        public string? Unit { get; set; }

        /// <summary>Clave de unidad — catálogo SAT c_ClaveUnidad. Ej. "H87", "MTS".</summary>
        public string? UnitCode { get; set; }

        /// <summary>Valor unitario antes de impuestos. Obligatorio.</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Cantidad. Obligatorio.</summary>
        public decimal Quantity { get; set; }

        /// <summary>Subtotal del concepto (UnitPrice × Quantity, antes de descuento e impuestos). Obligatorio.</summary>
        public decimal Subtotal { get; set; }

        /// <summary>Objeto de impuesto — catálogo SAT c_ObjetoImp. "01" (no objeto), "02" (sí objeto). Obligatorio.</summary>
        public string TaxObject { get; set; } = string.Empty;

        /// <summary>Datos de cuenta a nombre de terceros (opcional).</summary>
        public ThirdPartyAccountComplement? ThirdPartyAccount { get; set; }

        /// <summary>Descuento aplicado al concepto (opcional).</summary>
        public decimal? Discount { get; set; }

        /// <summary>Impuestos del concepto (traslados y retenciones).</summary>
        public List<Tax> Taxes { get; set; } = new();

        /// <summary>Números de cuenta predial para arrendamiento (opcional).</summary>
        public List<string>? PropertyTaxIDNumber { get; set; }

        /// <summary>Números de pedimento de importación (opcional).</summary>
        public List<string>? NumerosPedimento { get; set; }

        /// <summary>Total del concepto (subtotal − descuento + impuestos). Obligatorio.</summary>
        public decimal Total { get; set; }

        /// <summary>Complemento a nivel concepto (opcional).</summary>
        public ItemComplement? Complement { get; set; }
    }
}
