namespace FacturamaNetSDK.Models.Common.Catalogs
{
    public sealed class ProductService : CatalogBase
    {
        public string IncludeIva { get;  }
        public string IncludeIeps { get;  }
        public string Complement { get;  }

        public string DangerousMaterial { get;  }
    }
}
