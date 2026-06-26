using Facturama.Sdk.Core.Models.Filters;
using Facturama.Sdk.Core.Models.Retentions.Response;
using FacturamaNetSDk.Enums;
using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Cfdi.Responses;
using FacturamaNetSDK.Models.Retentions.Request;
using FacturamaNetSDK.Utilities;
using Microsoft.VisualBasic.FileIO;

namespace FacturamaNetSDK.Endpoints
{
    public sealed class RetentionEndpoint :IRetentionEndpoint
    {
        private readonly string Resource = "retenciones";
        private readonly FacturamaHttpClient _client;
    

        internal RetentionEndpoint(FacturamaHttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Crear un cfdi de tipo retención
        /// </summary> <param name="request"></param>

        public Task<RetentionResponse> CreateAsync(
            RetentionRequest request,
            CancellationToken cancellationToken = default)

        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            return _client.PostAsync<RetentionResponse>(
                $"2/{Resource}",
                request,
                cancellationToken: cancellationToken);
        }

        public Task<RetentionResponse> GetAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            return _client.GetAsync<RetentionResponse>(
                $"{Resource}/{id}",
                cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<CfdiListResponse>> ListAsync(
            RetentionFilter filters,
            CancellationToken cancellationToken = default)
        {
            var queryParams = filters != null
                   ? QueryBuilder.FromObject(filters)
                   : null;

            var result = await _client.GetAsync<List<CfdiListResponse>>(
                Resource, queryParams, cancellationToken);

            if (result is null) return Array.Empty<CfdiListResponse>();
            return result.AsReadOnly();
        }

        public async Task<CfdiCancellationResponse> CancelAsync(
            string id,
            string? motive = null,
            string? uuidReplacement = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            var queryParams = new Dictionary<string, string?>();

            if (!string.IsNullOrWhiteSpace(motive))
                queryParams["motive"] = motive;

            if (!string.IsNullOrWhiteSpace(uuidReplacement))
                queryParams["uuidReplacement"] = uuidReplacement;

            return await _client.DeleteAsync<CfdiCancellationResponse>(
                $"{Resource}/{id}",
                queryParams,
                cancellationToken);

        }
        public async Task<CfdiSendResponse> SendByEmailAsync(
            string id,
            string email,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(email, nameof(email));
            var queryParams = new Dictionary<string, string?>
            {
                ["id"] = id,
                ["email"] = email
            };
            return await _client.PostAsync<CfdiSendResponse>(
                $"{Resource}/envia",
                null,
                queryParams,
                cancellationToken);
        }

        public async Task<CfdiDownloadResponse> DownloadAsync(
            string fileType,
            string id,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(fileType, nameof(fileType));

            return await _client.GetAsync<CfdiDownloadResponse>(
                $"{Resource}/{id}/{fileType}",
                null,
                cancellationToken);
        }

    }
}
