namespace FacturamaNetSDK.Models.Retentions.Request
{
    public sealed class ReceptorRequest
    {

		public string Nacionalidad { get; set; }	
        public NacionalRequest Nacional { get; set; }
        public ExtranjeroRequest Extranjero { get; set; }
    }
}
