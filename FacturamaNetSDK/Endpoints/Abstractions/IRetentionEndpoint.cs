using Facturama.Sdk.Core.Models.Filters;
using Facturama.Sdk.Core.Models.Retentions.Response;
using FacturamaNetSDk.Enums;
using FacturamaNetSDK.Models.Cfdi.Responses;
using FacturamaNetSDK.Models.Retentions.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturamaNetSDK.Endpoints.Abstractions
{
    public interface IRetentionEndpoint
    {
        Task<RetentionResponse> CreateAsync(
            RetentionRequest request,
            CancellationToken cancellationToken = default);

        Task<RetentionResponse> GetAsync(
            string id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CfdiListResponse>> ListAsync(
            RetentionFilter filter,
            CancellationToken cancellationToken = default);

        Task<CfdiCancellationResponse>CancelAsync(
               string id,
                string? motive = null,
                string? uuidReplacement = null,
                CancellationToken cancellationToken = default);

        Task<CfdiSendResponse> SendByEmailAsync(
            string id,
            string email,
            CancellationToken cancellationToken = default);

        Task<CfdiDownloadResponse> DownloadAsync(
            string fileType,
            string id,
            CancellationToken cancellationToken = default);
    }
}
