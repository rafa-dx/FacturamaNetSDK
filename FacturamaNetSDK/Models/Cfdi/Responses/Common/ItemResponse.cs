namespace FacturamaNetSDK.Models.Cfdi.Responses.Common;

/// <summary>
/// Concepto (partida) de un CFDI en las respuestas de la API.
/// Compartido entre CFDI Web y CFDI Lite (estructura idéntica en ambas).
/// </summary>
public record ItemResponse
{
    public string? ProductCode { get; init; }
    public string? IdentificationNumber { get; init; }
    public string? UnitCode { get; init; }
    public decimal? Discount { get; init; }
    public string? CuentaPredial { get; init; }
    public decimal? Quantity { get; init; }
    public string? Unit { get; init; }
    public string? Description { get; init; }
    public decimal? UnitValue { get; init; }
    public decimal? Total { get; init; }
}
