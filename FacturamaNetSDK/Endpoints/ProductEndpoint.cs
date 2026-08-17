using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Models.Product.Request;
using FacturamaNetSDK.Models.Product.Response;
using FacturamaNetSDK.Utilities;

namespace FacturamaNetSDK.Endpoints
{
    /// <summary>
    /// Operaciones de productos (/Product).
    /// </summary>
    public sealed class ProductEndpoint : IProductEndpoint
    {
        private const string Resource = "product";
        private const string ProductsResource = "products";

        private readonly FacturamaHttpClient _client;

        internal ProductEndpoint(FacturamaHttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc />
        public Task<ProductResponse> CreateAsync(
            ProductRequest request,
            string? idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            return _client.PostAsync<ProductResponse>(
                Resource,
                request,
                idempotencyKey: idempotencyKey,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<ProductResponse> DeleteAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            EnsureId(id);
            return _client.DeleteAsync<ProductResponse>(
                $"{Resource}/{id}",
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<ProductResponse> GetAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            EnsureId(id);
            return _client.GetAsync<ProductResponse>(
                $"{Resource}/{id}",
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<ProductResponse> UpdateAsync(
            string id,
            ProductRequest request,
            CancellationToken cancellationToken = default)
        {
            EnsureId(id);
            ArgumentNullException.ThrowIfNull(request);
            return _client.PutAsync<ProductResponse>(
                $"{Resource}/{id}",
                request,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<FilterProductResponse> ListAsync(
            QueryOptions? filters = null,
            CancellationToken cancellationToken = default)
        {
            var queryParams = filters is not null
                ? QueryBuilder.FromObject(filters)
                : null;

            return _client.GetAsync<FilterProductResponse>(
                ProductsResource,
                queryParams,
                cancellationToken);
        }

        private static void EnsureId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El ID no puede estar vacío.", nameof(id));
        }
    }
}
