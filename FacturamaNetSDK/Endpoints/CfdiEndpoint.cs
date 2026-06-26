
using FacturamaNetSDk.Enums;
using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Cfdi.Requests;
using FacturamaNetSDK.Models.Cfdi.Responses;
using FacturamaNetSDK.Models.Cfdi.Responses.CfdiWeb;
using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Utilities;


namespace FacturamaNetSDK.Endpoints;

/// <summary>
/// Operaciones CFDI para API Web (/api/3/cfdis).
/// </summary>
public sealed class CfdiEndpoint : ICfdiEndpoint
{
    private const string Resource = "cfdis";
    private readonly FacturamaHttpClient _client;

    internal CfdiEndpoint(FacturamaHttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Crea un nuevo CFDI.
    /// </summary>
    public  Task<CfdiResponse> CreateAsync(
        CfdiRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return  _client.PostAsync<CfdiResponse>(
            Resource, 
            request, 
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Obtiene un CFDI por ID.
    /// </summary>
    public async Task<CfdiResponse> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {

        if(string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("El ID no puede estar vacío.", nameof(id));
        }

        return await _client.GetAsync<CfdiResponse>(
            $"/cfdi/{id}",
            queryParams: new() { ["type"] = InvoiceType.Issued.ToString().ToLower() },
            cancellationToken);
    }

    /// <summary>
    /// Lista CFDIs con filtros opcionales.
    /// </summary>
    public async Task<IReadOnlyList<CfdiListResponse>?> ListAsync(
        CfdiFilter? filters = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = filters != null
               ? QueryBuilder.FromObject(filters)
               : null;

        var result = await _client.GetAsync<List<CfdiListResponse>>(
            "/cfdi", 
            queryParams, 
            cancellationToken);

        if (result is null) return Array.Empty<CfdiListResponse>();
        return result.AsReadOnly();
    }

    /// <summary>
    /// Consulta el status de un CFDI en el SAT.
    /// </summary>
    public async Task<CfdiStatusResponse> GetStatusAsync(
        CfdiStatusParams statusParams,
        CancellationToken cancellationToken = default)
    {

        var queryParams = statusParams != null
              ? QueryBuilder.FromObject(statusParams)
              : null;

        return await _client.GetAsync<CfdiStatusResponse>(
            $"/cfdi/status",
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
            $"/cfdi/{fileType}/{invoiceType}/{id}",
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
            $"/cfdi/{id}",
            queryParams: new()
            {
                ["type"] = invoiceType.ToString().ToLower(),
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
        InvoiceType invoiceType = InvoiceType.Issued,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El ID no puede estar vacío.", nameof(id));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El correo electrónico no puede estar vacío.", nameof(email));
        return _client.PostAsync<CfdiSendResponse>(
            "/cfdi",
            new { },
            queryParams: new()
            {
                ["cfdiType"] = invoiceType.ToString().ToLower(),
                ["cfdiId"] = id,
                ["email"] = email
            },
            cancellationToken);
    }
}