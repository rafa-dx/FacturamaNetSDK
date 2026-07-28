
using FacturamaNetSDK.Enums;
using FacturamaNetSDK.Models.Cfdi.Requests;
using FacturamaNetSDK.Models.Cfdi.Responses;
using FacturamaNetSDK.Models.Cfdi.Responses.CfdiWeb;
using FacturamaNetSDK.Models.Filters;

namespace FacturamaNetSDK.Endpoints.Abstractions;

/// <summary>
/// Operaciones CFDI para API Web.
/// </summary>
public interface ICfdiEndpoint
{
    /// <summary>Crea un nuevo CFDI.</summary>
    Task<CfdiResponse> CreateAsync(
        CfdiRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene un CFDI por ID.</summary>
    Task<CfdiResponse> GetAsync(
        string id, 
        CancellationToken cancellationToken = default);

    /// <summary>Lista CFDIs con filtros opcionales.</summary>
    Task<IReadOnlyList<CfdiListResponse>> ListAsync(
        CfdiFilter? filters = null,
        CancellationToken cancellationToken = default);

    /// <summary>Consulta el status de un CFDI en el SAT.</summary>
    Task<CfdiStatusResponse> GetStatusAsync(
        CfdiStatusParams statusParams, 
        CancellationToken cancellationToken = default);

    /// <summary>Descarga un archivo del CFDI (XML o PDF) en Base64.</summary>
    Task<CfdiDownloadResponse> DownloadAsync(
        CfdiFileType fileType, 
        InvoiceType invoiceType, 
        string id, 
        CancellationToken cancellationToken = default);

    /// <summary>Cancela un CFDI.</summary>
    Task<CfdiCancellationResponse> CancelAsync(
        string id, 
        InvoiceType invoiceType, 
        string? motive = null, 
        string? uuidReplacement = null, 
        CancellationToken cancellationToken = default);

    /// <summary>Envía un CFDI por correo electrónico.</summary>
    Task<CfdiSendResponse> SendByEmailAsync(
        string id, 
        string email, 
        InvoiceType invoiceType, 
         CancellationToken cancellationToken = default);
}