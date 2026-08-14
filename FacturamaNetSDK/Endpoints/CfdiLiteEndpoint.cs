
using FacturamaNetSDK.Enums;
using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Cfdi.Requests;
using FacturamaNetSDK.Models.Cfdi.Responses;
using FacturamaNetSDK.Models.Cfdi.Responses.CfdiLite;
using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Utilities;

namespace FacturamaNetSDK.Endpoints;

/// <summary>
/// Operaciones CFDI para API Lite (/api-lite/{version}/cfdis).
/// </summary>
public sealed class CfdiLiteEndpoint : ICfdiLiteEndpoint
{
    private const string Resource = "cfdis";
    private readonly FacturamaHttpClient _liteVersionedClient; // api-lite/{version}/
    private readonly FacturamaHttpClient _liteClient;          // api-lite/
    private readonly FacturamaHttpClient _rootClient;          // raíz /
    private readonly FacturamaHttpClient _webClient;           // api/3/

    internal CfdiLiteEndpoint(
         FacturamaHttpClient liteVersionedClient,
         FacturamaHttpClient liteClient,
         FacturamaHttpClient rootClient,
         FacturamaHttpClient webClient)
    {
        _liteVersionedClient = liteVersionedClient ?? throw new ArgumentNullException(nameof(liteVersionedClient));
        _liteClient = liteClient ?? throw new ArgumentNullException(nameof(liteClient));
        _rootClient = rootClient ?? throw new ArgumentNullException(nameof(rootClient));
        _webClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
    }

    /// <summary>
    /// Crea un nuevo CFDI.
    /// </summary>
    public Task<CfdiLiteResponse> CreateAsync(
        CfdiRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _liteVersionedClient.PostAsync<CfdiLiteResponse>(
            Resource,
            request,
            idempotencyKey: idempotencyKey,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Obtiene un CFDI por ID.
    /// </summary>
    public Task<CfdiLiteResponse> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));


        return _liteClient.GetAsync<CfdiLiteResponse>(
            $"{Resource}/{id}",
            queryParams: new() { ["type"] = "issuedLite" },
            cancellationToken);
    }

    /// <summary>
    /// Lista CFDIs con filtros opcionales.
    /// </summary>
    public async Task<IReadOnlyList<CfdiListResponse>> ListAsync(
        CfdiFilter? filters = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = filters is not null
            ? QueryBuilder.FromObject(filters)
            : null;

        var result = await _rootClient.GetAsync<List<CfdiListResponse>>(
            "/cfdi", queryParams, cancellationToken).ConfigureAwait(false);

        if (result is null) return Array.Empty<CfdiListResponse>();
        return result.AsReadOnly();
    }

    /// <summary>
    /// Descarga un archivo del CFDI (XML o PDF) en Base64.
    /// </summary>
    public Task<CfdiDownloadResponse> DownloadAsync(
        CfdiFileType fileType,
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        return _webClient.GetAsync<CfdiDownloadResponse>(
            $"/api/cfdi/{fileType}/{InvoiceType.IssuedLite.ToApiValue()}/{id}",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Cancela un CFDI.
    /// </summary>
    public Task<CfdiCancellationResponse> CancelAsync(
        string id,
        string? motive = null,
        string? uuidReplacement = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        return _liteClient.DeleteAsync<CfdiCancellationResponse>(
            $"/api-lite/{Resource}/{id}",
            queryParams: new()
            {
                ["type"] = InvoiceType.IssuedLite.ToApiValue(),
                ["motive"] = motive,
                ["uuidReplacement"] = uuidReplacement
            },
            cancellationToken);
    }

    /// <summary>
    /// Envía un CFDI por correo electrónico.
    /// </summary>
    public Task<CfdiSendResponse> SendByEmailAsync(
        string id,
        string email,
        string? subject = null,
        string? comments = null,
        string? issuerEmail = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));
        return _rootClient.PostAsync<CfdiSendResponse>(
            "/cfdi",
            new { },
            queryParams: new()
            {
                ["cfdiType"] = InvoiceType.IssuedLite.ToApiValue(),
                ["cfdiId"] = id,
                ["email"] = email,
                ["subject"] = subject,
                ["comments"] = comments,
                ["issuerEmail"] = issuerEmail
            },
            cancellationToken: cancellationToken);
    }
}