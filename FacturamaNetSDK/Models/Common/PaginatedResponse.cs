namespace FacturamaNetSDK.Models.Common
{
    public sealed class  PaginatedResponse<T>
    {

        public  IReadOnlyList<T> Items { get; set; }


        /// <summary>Total de registros sin filtros</summary>

        public  int recordsTotal { get; set; }

        /// <summary>Total de registros después de aplicar filtros</summary>

        public  int recordsFiltered { get; set; }

        /// <summary>Índice inicial solicitado</summary>
        //public required int Start { get; set; }

        /// <summary>Cantidad solicitada</summary>
        //public required int Length { get; set; }
    }
}
