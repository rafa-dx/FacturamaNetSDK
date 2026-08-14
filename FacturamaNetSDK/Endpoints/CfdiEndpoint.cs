
using FacturamaNetSDK.Enums;
using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Cfdi.Requests;
using FacturamaNetSDK.Models.Cfdi.Responses;
using FacturamaNetSDK.Models.Cfdi.Responses.CfdiWeb;
using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Utilities;


namespace FacturamaNetSDK.Endpoints;

/// <summary>
/// Operaciones CFDI para API Web (/3/cfdis).
/// </summary>
public sealed class CfdiEndpoint : ICfdiEndpoint
{
    private const string CfdisResource = "cfdis";
    private const string CfdiResource = "cfdi";

    private readonly FacturamaHttpClient _client;

    internal CfdiEndpoint(FacturamaHttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Crea un nuevo CFDI.
    /// </summary>
    public Task<CfdiResponse> CreateAsync(
        CfdiRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.PostAsync<CfdiResponse>(
            CfdisResource,
            request,
            idempotencyKey: idempotencyKey,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Obtiene un CFDI por ID.
    /// </summary>
    public Task<CfdiResponse> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El ID no puede estar vacío.", nameof(id));

        return _client.GetAsync<CfdiResponse>(
            $"/{CfdiResource}/{id}",
            queryParams: new() { ["type"] = InvoiceType.Issued.ToString().ToLower() },
            cancellationToken);
    }

    /// <summary>
    /// Lista CFDIs con filtros opcionales.
    /// </summary>
    public async Task<IReadOnlyList<CfdiListResponse>> ListAsync(
        CfdiFilter? filters = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = filters != null
               ? QueryBuilder.FromObject(filters)
               : null;

        var result = await _client.GetAsync<List<CfdiListResponse>>(
            $"/{CfdiResource}",
            queryParams,
            cancellationToken).ConfigureAwait(false);

        if (result is null) return Array.Empty<CfdiListResponse>();
        return result.AsReadOnly();
    }

    /// <summary>
    /// Consulta el status de un CFDI en el SAT.
    /// </summary>
    public Task<CfdiStatusResponse> GetStatusAsync(
        CfdiStatusParams statusParams,
        CancellationToken cancellationToken = default)
    {
        var queryParams = statusParams is not null
            ? QueryBuilder.FromObject(statusParams)
            : null;

        return _client.GetAsync<CfdiStatusResponse>(
            $"/{CfdiResource}/status",
            queryParams,
            cancellationToken);
    }

    /// <summary>
    /// Descarga un archivo del CFDI (XML o PDF) en Base64.
    /// </summary>
    public Task<CfdiDownloadResponse> DownloadAsync(
        CfdiFileType fileType,
        InvoiceType invoiceType,
        string id,
        CancellationToken cancellationToken = default)
    {

        if(string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El ID no puede estar vacío.", nameof(id));

        return _client.GetAsync<CfdiDownloadResponse>(
            $"/{CfdiResource}/{fileType}/{invoiceType}/{id}",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Cancela un CFDI.
    /// </summary>
    public  Task<CfdiCancellationResponse> CancelAsync(
        string id,
        InvoiceType invoiceType,
        string? motive = null,
        string? uuidReplacement = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El ID no puede estar vacío.", nameof(id));

        return _client.DeleteAsync<CfdiCancellationResponse>(
            $"/{CfdiResource}/{id}",
            queryParams: new()
            {
                ["type"] = invoiceType.ToString().ToLower(),
                ["motive"] = motive,
                ["uuidReplacement"] = uuidReplacement
            },
            cancellationToken : cancellationToken);
    }

    /// <summary>
    /// Envía un CFDI por correo electrónico.
    /// </summary>
    public Task<CfdiSendResponse> SendByEmailAsync(
        string id,
        string email,
        InvoiceType invoiceType = InvoiceType.Issued,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El ID no puede estar vacío.", nameof(id));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El correo electrónico no puede estar vacío.", nameof(email));
        return _client.PostAsync<CfdiSendResponse>(
            $"/{CfdiResource}",
            new { },
            queryParams: new()
            {
                ["cfdiType"] = invoiceType.ToString().ToLower(),
                ["cfdiId"] = id,
                ["email"] = email
            },
            cancellationToken: cancellationToken);
    }
}