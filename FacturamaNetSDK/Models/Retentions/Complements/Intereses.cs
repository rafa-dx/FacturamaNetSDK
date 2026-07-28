using System;

namespace FacturamaNetSDK.Models.Retentions.Complements
{
    public sealed class Intereses
    {
        public string Version { get; set; }
        public InteresesSistFinanciero SistFinanciero { get; set; }
        public InteresesRetiroAORESRetInt RetiroAORESRetInt { get; set; }
        public InteresesOperFinancDerivad OperFinancDerivad { get; set; }
        public decimal MontIntNominal { get; set; }
        public decimal MontIntReal { get; set; }
        public decimal Perdida { get; set; }
    }
    public enum InteresesSistFinanciero
    {

        /// <remarks/>
        SI,

        /// <remarks/>
        NO,
    }
    public enum InteresesRetiroAORESRetInt
    {

        /// <remarks/>
        SI,

        /// <remarks/>
        NO,
    }
    public enum InteresesOperFinancDerivad
    {

        /// <remarks/>
        SI,

        /// <remarks/>
        NO,
    }
}
