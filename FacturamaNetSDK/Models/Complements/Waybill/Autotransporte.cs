namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class Autotransporte
    {

        public string PermSCT { get; set; }
        public string NumPermisoSCT { get; set; }
        public Seguros Seguros { get; set; }
        public IdentificacionVehicular IdentificacionVehicular { get; set; }
        public Remolque[] Remolques { get; set; }
    }
}
