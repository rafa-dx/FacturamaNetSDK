using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Models.Product.Request;
using FacturamaNetSDK.Models.Product.Response;

namespace FacturamaNetSDK.Endpoints.Abstractions
{
    /// <summary>
    /// Operaciones de productos (/Product).
    /// </summary>
    public interface IProductEndpoint
    {
        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        /// <param name="request">Datos del producto a crear.</param>
        /// <param name="idempotencyKey">
        /// Clave de idempotencia para evitar duplicados si la petición se reintenta.
        /// Si se omite, el SDK genera una automáticamente.
        /// </param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task<ProductResponse> CreateAsync(
            ProductRequest request,
            string? idempotencyKey = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina un producto por ID.
        /// </summary>
        Task<ProductResponse> DeleteAsync(
            string id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un producto por ID.
        /// </summary>
        Task<ProductResponse> GetAsync(
            string id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        Task<ProductResponse> UpdateAsync(
            string id,
            ProductRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lista productos de forma paginada.
        /// </summary>
        /// <param name="filters">Opciones de paginación y búsqueda. Si se omite, se usan los valores por defecto.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task<FilterProductResponse> ListAsync(
            QueryOptions? filters = null,
            CancellationToken cancellationToken = default);
    }
}
