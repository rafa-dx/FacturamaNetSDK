using FacturamaNetSDK.Models.Cfdi.Responses;
using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Models.Retentions.Request;
using FacturamaNetSDK.Models.Retentions.Response;

namespace FacturamaNetSDK.Endpoints.Abstractions;

/// <summary>
/// Operaciones de retenciones.
/// </summary>
public interface IRetentionEndpoint
{
    /// <summary>Crea un CFDI de tipo retención.</summary>
    Task<RetentionResponse> CreateAsync(RetentionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Obtiene una retención por ID.</summary>
    Task<RetentionResponse> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Lista retenciones aplicando filtros opcionales.</summary>
    Task<IReadOnlyList<CfdiListResponse>> ListAsync(RetentionFilter filter, CancellationToken cancellationToken = default);

    /// <summary>Cancela una retención por ID.</summary>
    Task<CfdiCancellationResponse> CancelAsync(string id, string? motive = null, string? uuidReplacement = null, CancellationToken cancellationToken = default);

    /// <summary>Envía una retención por correo electrónico.</summary>
    Task<CfdiSendResponse> SendByEmailAsync(string id, string email, CancellationToken cancellationToken = default);

    /// <summary>Descarga una retención (PDF/XML) por ID.</summary>
    Task<CfdiDownloadResponse> DownloadAsync(string fileType, string id, CancellationToken cancellationToken = default);
}
