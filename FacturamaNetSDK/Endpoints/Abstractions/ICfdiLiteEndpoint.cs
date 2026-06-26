using FacturamaNetSDk.Enums;
using FacturamaNetSDK.Models.Cfdi.Requests;
using FacturamaNetSDK.Models.Cfdi.Responses;
using FacturamaNetSDK.Models.Cfdi.Responses.CfdiLite;
using FacturamaNetSDK.Models.Filters;

namespace FacturamaNetSDK.Endpoints.Abstractions;

/// <summary>
/// Operaciones CFDI para API Lite.
/// </summary>
public interface ICfdiLiteEndpoint
{
    /// <summary>Crea un nuevo CFDI.</summary>
    Task<CfdiLiteResponse> CreateAsync(
        CfdiRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene un CFDI por ID.</summary>
    Task<CfdiLiteResponse> GetAsync(
        string id, 
        CancellationToken cancellationToken = default);

    /// <summary>Lista CFDIs con filtros opcionales.</summary>
    Task<IReadOnlyList<CfdiListResponse>> ListAsync(
        CfdiFilter? filters = null,
        CancellationToken cancellationToken = default);

    /// <summary>Descarga un archivo del CFDI (XML o PDF) en Base64.</summary>
    Task<CfdiDownloadResponse> DownloadAsync(
        CfdiFileType fileType,
        string id, 
        CancellationToken cancellationToken = default);

    /// <summary>Cancela un CFDI.</summary>
    Task<CfdiCancellationResponse> CancelAsync(
        string id, 
        string? motive = null, 
        string? uuidReplacement = null, 
        CancellationToken cancellationToken = default);

    /// <summary>Envía un CFDI por correo electrónico.</summary>
    Task<CfdiSendResponse> SendByEmailAsync(
        string id, 
        string email,
        string? subject = null,
        string? comments = null,
        string? issuerEmail = null,
        CancellationToken cancellationToken = default);
}