
namespace FacturamaNetSDK.Models.Complements.TaxLegends
{
    public sealed class TaxLegendsComplement
    {
        public Legend[] Legends { get; set; }

    }
    public class Legend
    {

        public string TaxProvision { get; set; }
        public string Norm { get; set; }
        public string Text { get; set; }
    }
}

