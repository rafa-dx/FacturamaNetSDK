using System.Runtime.Serialization;


namespace FacturamaNetSDk.Enums
{
    public enum CfdiType
    {
        [EnumMember(Value = "I")]
        Ingreso,

        [EnumMember(Value = "E")]
        Egreso,

        [EnumMember(Value = "T")]
        Traslado,

        [EnumMember(Value = "P")]
        Pago,

        [EnumMember(Value = "N")]
        Nomina
    }

    public enum CfdiFileType
    {
        [EnumMember(Value = "xml")]
        Xml,
        [EnumMember(Value = "pdf")]
        Pdf,
        [EnumMember(Value = "html")]
        Html
    }

    public enum InvoiceType
    {
        [EnumMember(Value = "issued")]
        Issued,
        [EnumMember(Value = "received")]
        Received,
        [EnumMember(Value = "payroll")]
        Payroll,
        [EnumMember(Value = "issuedLite")]
        IssuedLite,
        [EnumMember(Value = "retention")]
        Retention

    }

    public enum InvoiceStatus
    {
        [EnumMember(Value = "all")]
        All,
        [EnumMember(Value = "active")]
        Active,
        [EnumMember(Value = "cancelled")]
        Cancelled
    }

}
