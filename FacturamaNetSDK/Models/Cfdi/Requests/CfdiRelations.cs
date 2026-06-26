namespace FacturamaNetSDK.Models.Cfdi.Requests;

public sealed class CfdiRelations
{
    public string? Type { get; set; }
    public List<CfdiRelation>? Cfdis { get; set; }
}

public sealed class CfdiRelation
{
    public string? Uuid { get; set; }
}
