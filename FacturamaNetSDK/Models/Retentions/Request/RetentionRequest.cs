namespace FacturamaNetSDK.Models.Retentions.Request
{
    public sealed class RetentionRequest 
    {
        public string FolioInt { get; set; }	
        public string FechaExp { get; set; }
        public string CveRetenc { get; set; }
        public string DescRetenc { get; set; }
        public EmisorRequest Emisor { get; set; }
        public ReceptorRequest Receptor { get; set; }
        public PeriodoRequest Periodo { get; set; }
        public TotalesRequest Totales { get; set; }
        public ComplementoRequest Complemento { get; set; }
        public string Id { get; set; }
        public string CadenaOriginal { get; set; }
        public bool IsCanceled { get; set; }
        public string Sello { get; set; }
        public string LugarExpRetenc { get; set; }
    }
}
