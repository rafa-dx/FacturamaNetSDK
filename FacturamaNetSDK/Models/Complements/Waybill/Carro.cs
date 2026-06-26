namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class Carro
    {
        public string TipoCarro { get; set; }

        public string MatriculaCarro { get; set; }

        public string GuiaCarro { get; set; }

        public decimal ToneladasNetasCarro { get; set; }

        public CarroContenedor[] Contenedor { get; set; }
    }
}
