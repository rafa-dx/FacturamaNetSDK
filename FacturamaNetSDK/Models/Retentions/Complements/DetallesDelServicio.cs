namespace FacturamaNetSDK.Models.Retentions.Complements
{
    public sealed class DetallesDelServicio
    {

        public ImpuestosTrasladadosdelServicio ImpuestosTrasladadosdelServicio { get; set; }
        public ContribucionGubernamental ContribucionGubernamental { get; set; }

        public ComisionDelServicio ComisionDelServicio { get; set; }

        public string FormaPagoServ { get; set; }

        public string TipoDeServ { get; set; }

        public string SubTipServ { get; set; }

        public string RfcTerceroAutorizado { get; set; }

        public string FechaServ { get; set; }

        public decimal PrecioServSinIva { get; set; }
    }
}
