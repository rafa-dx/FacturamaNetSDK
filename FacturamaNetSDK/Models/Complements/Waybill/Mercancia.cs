using System.Text.Json.Serialization;


namespace FacturamaNetSDK.Models.Complements.Waybill
{
    public sealed class Mercancia
    {
        [JsonPropertyName("BienesTransp")]
        public string BienesTransp { get; set; }

        [JsonPropertyName("ClaveSTCC")]
        public string ClaveSTCC { get; set; }

        [JsonPropertyName("Descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("Cantidad")]
        public decimal Cantidad { get; set; }

        [JsonPropertyName("ClaveUnidad")]
        public string ClaveUnidad { get; set; }

        [JsonPropertyName("Unidad")]
        public string Unidad { get; set; }

        [JsonPropertyName("Dimensiones")]
        public string Dimensiones { get; set; }

        [JsonPropertyName("MaterialPeligroso")]
        public string MaterialPeligroso { get; set; }

        [JsonPropertyName("CveMaterialPeligroso")]
        public string CveMaterialPeligroso { get; set; }

        [JsonPropertyName("Embalaje")]
        public string Embalaje { get; set; }

        [JsonPropertyName("DescripEmbalaje")]
        public string DescripEmbalaje { get; set; }

        [JsonPropertyName("SectorCOFEPRIS")]
        public string SectorCOFEPRIS { get; set; }

        [JsonPropertyName("NombreIngredienteActivo")]
        public string NombreIngredienteActivo { get; set; }

        [JsonPropertyName("NomQuimico")]
        public string NomQuimico { get; set; }

        [JsonPropertyName("DenominacionGenericaProd")]
        public string DenominacionGenericaProd { get; set; }

        [JsonPropertyName("DenominacionDistintivaProd")]
        public string DenominacionDistintivaProd { get; set; }

        [JsonPropertyName("Fabricante")]
        public string Fabricante { get; set; }

        [JsonPropertyName("FechaCaducidad")]
        public DateTime? FechaCaducidad { get; set; }

        [JsonPropertyName("LoteMedicamento")]
        public string LoteMedicamento { get; set; }

        [JsonPropertyName("FormaFarmaceutica")]
        public string FormaFarmaceutica { get; set; }

        [JsonPropertyName("CondicionesEspTransp")]
        public string CondicionesEspTransp { get; set; }

        [JsonPropertyName("RegistroSanitarioFolioAutorizacion")]
        public string RegistroSanitarioFolioAutorizacion { get; set; }

        [JsonPropertyName("PermisoImportacion")]
        public string PermisoImportacion { get; set; }

        [JsonPropertyName("FolioImpoVUCEM")]
        public string FolioImpoVUCEM { get; set; }

        [JsonPropertyName("NumCAS")]
        public string NumCas { get; set; }

        [JsonPropertyName("RazonSocialEmpImp")]
        public string RazonSocialEmpImp { get; set; }

        [JsonPropertyName("NumRegSanPlagCOFEPRIS")]
        public string NumRegSanPlagCOFEPRIS { get; set; }

        [JsonPropertyName("DatosFabricante")]
        public string DatosFabricante { get; set; }

        [JsonPropertyName("DatosFormulador")]
        public string DatosFormulador { get; set; }

        [JsonPropertyName("DatosMaquilador")]
        public string DatosMaquilador { get; set; }

        [JsonPropertyName("UsoAutorizado")]
        public string UsoAutorizado { get; set; }


        [JsonPropertyName("PesoEnKg")]
        public decimal PesoEnKg { get; set; }

        [JsonPropertyName("ValorMercancia")]
        public decimal ValorMercancia { get; set; }

        [JsonPropertyName("Moneda")]
        public string Moneda { get; set; }

        [JsonPropertyName("FraccionArancelaria")]
        public string FraccionArancelaria { get; set; }

        [JsonPropertyName("UUIDComercioExt")]
        public string UUIDComercioExt { get; set; }
        [JsonPropertyName("TipoMaterial")]
        public string TipoMaterial { get; set; }

        [JsonPropertyName("DescripcionMaterial")]
        public string DescripcionMaterial { get; set; }

        [JsonPropertyName("DocumentacionAduanera")]
        public DocumentacionAduanera[] DocumentacionAduanera { get; set; }


        [JsonPropertyName("Pedimentos")]
        public Pedimentos[] Pedimentos { get; set; }

        [JsonPropertyName("GuiasIdentificacion")]
        public GuiasIdentificacion[] GuiasIdentificacion { get; set; }

        [JsonPropertyName("CantidadTransporta")]
        public CantidadTransporta[] CantidadTransporta { get; set; }

        [JsonPropertyName("DetalleMercancia")]
        public DetalleMercancia DetalleMercancia { get; set; }
    }
}
