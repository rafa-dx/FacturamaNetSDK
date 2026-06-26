namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class Contenedor
    {
        public string MatriculaContenedor { get; set; }
        public string TipoContenedor { get; set; }
        public string NumPrecinto { get; set; }
        public string IdCCPRelacionado { get; set; }
        public string PlacaVMCCP { get; set; }
        public DateTime FechaCertificacionCCP { get; set; }

        public RemolquesCCP[] RemolquesCCP { get; set; }
    }
}
