namespace FacturamaNetSDK.Models.Product.Response
{
    /// <summary>
    /// Página de resultados devuelta por el listado paginado de productos.
    /// </summary>
    public sealed record FilterProductResponse
    {
        /// <summary>
        /// Total de productos existentes, sin aplicar el filtro de búsqueda.
        /// </summary>
        public int RecordsTotal { get; init; }

        /// <summary>
        /// Total de productos que coinciden con el filtro de búsqueda.
        /// </summary>
        public int RecordsFiltered { get; init; }

        /// <summary>
        /// Productos incluidos en la página solicitada.
        /// </summary>
        public IReadOnlyList<ProductResponse> Data { get; init; } = Array.Empty<ProductResponse>();
    }
}
