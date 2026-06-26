namespace FacturamaNetSDK.Models.Retentions.Complements
{
    public sealed class ServiciosPlataformasTecnologicas
    {

		public List<DetallesDelServicio> Servicios { get; set; }
        public string Periodicidad { get; set; }
        public int NumServ { get; set; }
        public decimal MontToServSIva { get; set; }
        public decimal TotalIvaTrasladado { get; set; }
        public decimal TotalIvaRetenido { get; set; }

        public decimal TotalIsrRetenido { get; set; }
        public decimal DifIvaEntregadoPrestServ { get; set; }

        public decimal MonTotalporUsoPlataforma { get; set; }
        public decimal? MonTotalContribucionGubernamental { get; set; }
    }
}
