using FacturamaNetSDK.Models.Complements;

namespace FacturamaNetSDK.Models.Cfdi.Requests;

public class CfdiRequest
{
    public string? NameId { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string? Serie { get; set; }
    public string? CfdiType { get; set; }
    public string? PaymentForm { get; set; }
    public string? PaymentMethod { get; set; }
    public string? OrderNumber { get; set; }
    public string ExpeditionPlace { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? PaymentConditions { get; set; }
    public string? Observations { get; set; }
    public string? Exportation { get; set; }
    public string? Currency { get; set; }
    public decimal? CurrencyExchangeRate { get; set; }
    public string? PaymentBankName { get; set; }
    public string? PaymentAccountNumber { get; set; }
    public CfdiRelations? Relations { get; set; }
    public GlobalInformation? GlobalInformation { get; set; }
    public Receiver Receiver { get; set; } = new();
    public List<Item> Items { get; set; } = new();
    public CfdiComplement? Complement { get; set; }
}