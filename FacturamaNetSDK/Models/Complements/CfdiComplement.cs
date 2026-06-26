
using Facturama.Sdk.Core.Models.Complements.ForeignTrade;
using FacturamaNetSDk.Models.Complements.TaxLegends;
using FacturamaNetSDK.Models.Complements.Donation;
using FacturamaNetSDK.Models.Complements.Payments;
using FacturamaNetSDK.Models.Complements.Payroll;
using FacturamaNetSDK.Models.Complements.Waybill;
using FacturamaNetSDK.Models.TaxEntity.Response;

namespace FacturamaNetSDK.Models.Complements
{
    public sealed class CfdiComplement
    {
   
        public DonationComplement? Donation { get; set; }
        public PaymentComplement? Payments { get; set; }
        public TaxStampResponse TaxStamp { get; set; }
        public PayrollComplement Payroll { get; set; }
        public ComplementoCartaPorte31 CartaPorte31 { get; set; }

        public TaxLegendsComplement TaxLegends { get; set; }


        public ForeignTradeComplement ForeignTrade { get; set; }





    }
}
