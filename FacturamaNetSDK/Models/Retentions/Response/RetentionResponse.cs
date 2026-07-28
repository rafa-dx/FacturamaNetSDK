using FacturamaNetSDK.Models.Retentions.Request;
using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Retentions.Response
{
    public record RetentionResponse
    {

        [JsonPropertyName("FolioInt")]
        public string FolioInt { get; init; }

        [JsonPropertyName("FechaExp")]
        public string FechaExp { get; init; }

        [JsonPropertyName("CveRetenc")]
        public string CveRetenc { get; init; }

        [JsonPropertyName("Emisor")]
        public EmisorResponse Emisor { get; init; }

        [JsonPropertyName("Receptor")]
        public ReceptorResponse Receptor { get; init; }

        [JsonPropertyName("Periodo")]
        public PeriodoResponse Periodo { get; init; }

        [JsonPropertyName("Totales")]
        public TotalResponse Totales { get; init; }

        [JsonPropertyName("Complemento")]
        public ComplementoResponse Complemento { get; init; }

        [JsonPropertyName("Id")]
        public string Id { get; init; }

        [JsonPropertyName("CadenaOriginal")]
        public string CadenaOriginal { get; init; }

        [JsonPropertyName("IsCanceled")]
        public bool IsCanceled { get; init; }

        [JsonPropertyName("Sello")]
        public string Sello { get; init; }

        [JsonPropertyName("LugarExpRetenc")]
        public string LugarExpRetenc { get; init; } 
    }

    public record EmisorResponse
    {
	
        public string RfcEmisor { get; init; }

        public string NomDenRazSocE { get; init; }
	
        public string CurpE { get; init; }

        public string RegimenFiscalE { get; init; }
    }

    public record ReceptorResponse
    {
	
        public string Nacionalidad { get; init; }
	
        public NacionalResponse Nacional { get; init; }

        public ExtranjeroRequest Extranjero { get; init; }
    }

    public record NacionalResponse
    {

        public string RfcRecep { get; init; }
        public string NomDenRazSocR { get; init; }	
        public string CurpR { get; init; }
        public string DomicilioFiscalR { get; init; }
    }

    public record PeriodoResponse
    {
        public int MesIni { get; init; }	
        public int MesFin { get; init; }
        public int Ejerc { get; init; }
    }

    public record TotalResponse
    {
	
        public decimal MontoTotOperacion { get; init; }
        public decimal MontoTotGrav { get; init; }
        public decimal MontoTotExent { get; init; }
        public decimal MontoTotRet { get; init; }
        public List<ImpRetenido> ImpRetenidos { get; init; }

    }

    public record ImpRetenido
    {	
        public decimal BaseRet { get; init; }
        public string Impuesto { get; init; }

        public decimal MontoRet { get; init; }	
        public string TipoPagoRet { get; init; }
    }

    public record ComplementoResponse
    {
        public ServiciosPlataformasTecnologicasResponse? ServiciosPlataformasTecnologicas { get; set; }
        public TimbreFiscalDigitalResponse TimbreFiscalDigital { get; set; }
        public InteresesResponse Intereses { get; set; }
    }

    public record ServiciosPlataformasTecnologicasResponse
    {
        public List<DetallesDelServicioResponse> Servicios { get; set; }
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

    public record TimbreFiscalDigitalResponse
    {
        public string Version { get; set; }
        public string UUID { get; set; }
        public string FechaTimbrado { get; set; }
        public string SelloCFD { get; set; }
        public string NoCertificadoSAT { get; set; }
        public string SelloSAT { get; set; }
        public string RfcProvCertif {  get; set; }
    }

    public record InteresesResponse
    {
        public string Version { get; set; }
        public string SistFinanciero { get; set; }
        public string RetiroAORESRetInt { get; set; }
        public string OperFinancDerivad { get; set; }
        public decimal MontIntNominal { get; set; }
        public decimal MontIntReal { get; set; }
        public decimal Perdida { get; set; }

    }

    public record DetallesDelServicioResponse
    {
        public ImpuestosTrasladadosdelServicioResponse ImpuestosTrasladadosdelServicio { get; set; }
        public ContribucionGubernamentalResponse ContribucionGubernamental { get; set; }
        public ComisionDelServicioResponse ComisionDelServicio { get; set; }
        public string FormaPagoServ { get; set; }
        public string TipoDeServ { get; set; }
        public string SubTipServ { get; set; }
        public string RfcTerceroAutorizado { get; set; }
        public string FechaServ { get; set; }
        public decimal PrecioServSinIva { get; set; }
    }

    public record ImpuestosTrasladadosdelServicioResponse
    {
        public decimal Base { get; set; }
        public string Impuesto { get; set; }
        public string TipoFactor { get; set; }
        public decimal TasaCuota { get; set; }
        public decimal Importe { get; set; }

    }

    public record ContribucionGubernamentalResponse
    {
        public decimal ImpContrib { get; set; }
        public string EntidadDondePagaLaContribucion { get; set; }
    }
    public record ComisionDelServicioResponse
    {
		public decimal? Base { get; set; }
        public decimal? Porcentaje { get; set; }
        public decimal Importe { get; set; }
    }
}
