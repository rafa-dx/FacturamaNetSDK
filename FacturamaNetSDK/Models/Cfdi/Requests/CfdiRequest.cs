using FacturamaNetSDK.Models.Complements;

namespace FacturamaNetSDK.Models.Cfdi.Requests;

/// <summary>
/// Petición para emitir un CFDI 4.0 a través de la API Web (<c>POST /api/3/cfdis</c>).
/// </summary>
public class CfdiRequest
{
    /// <summary>Identificador del tipo de comprobante en Facturama (opcional). Ej. "1".</summary>
    public string? NameId { get; set; }

    /// <summary>Folio del comprobante. Obligatorio.</summary>
    public string Folio { get; set; } = string.Empty;

    /// <summary>Serie del comprobante (opcional). Ej. "FAC".</summary>
    public string? Serie { get; set; }

    /// <summary>Tipo de comprobante — catálogo SAT c_TipoDeComprobante (I, E, T, N, P). Usar <c>CfdiType.X.ToApiValue()</c>.</summary>
    public string? CfdiType { get; set; }

    /// <summary>Forma de pago — catálogo SAT c_FormaPago. Ej. "01" (efectivo), "03" (transferencia).</summary>
    public string? PaymentForm { get; set; }

    /// <summary>Método de pago — catálogo SAT c_MetodoPago. "PUE" (una exhibición) o "PPD" (parcialidades/diferido).</summary>
    public string? PaymentMethod { get; set; }

    /// <summary>Número de orden o referencia interna (opcional).</summary>
    public string? OrderNumber { get; set; }

    /// <summary>Lugar de expedición (código postal del emisor). Obligatorio. Ej. "78000".</summary>
    public string ExpeditionPlace { get; set; } = string.Empty;

    /// <summary>Fecha de emisión en formato ISO 8601 (<c>yyyy-MM-ddTHH:mm:ss</c>). Obligatorio.</summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>Condiciones de pago en texto libre (opcional).</summary>
    public string? PaymentConditions { get; set; }

    /// <summary>Observaciones; se muestran solo en el PDF (opcional).</summary>
    public string? Observations { get; set; }

    /// <summary>Exportación — catálogo SAT c_Exportacion. Ej. "01" (no aplica).</summary>
    public string? Exportation { get; set; }

    /// <summary>Moneda — catálogo SAT c_Moneda. Ej. "MXN", "USD".</summary>
    public string? Currency { get; set; }

    /// <summary>Tipo de cambio respecto a la moneda (requerido si la moneda no es MXN).</summary>
    public decimal? CurrencyExchangeRate { get; set; }

    /// <summary>Nombre del banco de pago (opcional).</summary>
    public string? PaymentBankName { get; set; }

    /// <summary>Número de cuenta de pago (opcional).</summary>
    public string? PaymentAccountNumber { get; set; }

    /// <summary>CFDI relacionados (sustituciones, notas de crédito, etc.). Opcional.</summary>
    public CfdiRelations? Relations { get; set; }

    /// <summary>Información global para facturas al público en general (opcional).</summary>
    public GlobalInformation? GlobalInformation { get; set; }

    /// <summary>Datos fiscales del receptor. Obligatorio.</summary>
    public Receiver Receiver { get; set; } = new();

    /// <summary>Conceptos (partidas) del comprobante. Debe contener al menos uno.</summary>
    public List<Item> Items { get; set; } = new();

    /// <summary>Complemento del CFDI (comercio exterior, nómina, pagos, etc.). Opcional.</summary>
    public CfdiComplement? Complement { get; set; }
}
