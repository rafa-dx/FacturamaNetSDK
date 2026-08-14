using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Cfdi.Responses;
using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Models.Retentions.Request;
using FacturamaNetSDK.Models.Retentions.Response;
using FacturamaNetSDK.Utilities;

namespace FacturamaNetSDK.Endpoints;

/// <summary>
/// Operaciones de retenciones (/api/2/retenciones).
/// </summary>
public sealed class RetentionEndpoint : IRetentionEndpoint
{
    private const string Resource = "retenciones";
    private readonly FacturamaHttpClient _client;

    internal RetentionEndpoint(FacturamaHttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Crea un CFDI de tipo retención.
    /// </summary>
    public Task<RetentionResponse> CreateAsync(
        RetentionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.PostAsync<RetentionResponse>(
            $"2/{Resource}",
            request,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Obtiene una retención por ID.
    /// </summary>
    public Task<RetentionResponse> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El id es obligatorio.", nameof(id));
        return _client.GetAsync<RetentionResponse>(
            $"{Resource}/{id}",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Lista retenciones aplicando filtros opcionales.
    /// </summary>
    public async Task<IReadOnlyList<CfdiListResponse>> ListAsync(
        RetentionFilter filters,
        CancellationToken cancellationToken = default)
    {
        var queryParams = filters is not null
            ? QueryBuilder.FromObject(filters)
            : null;

        var result = await _client.GetAsync<List<CfdiListResponse>>(
            Resource, queryParams, cancellationToken).ConfigureAwait(false);

        return result is null ? Array.Empty<CfdiListResponse>() : result.AsReadOnly();
    }

    /// <summary>
    /// Cancela una retención por ID.
    /// </summary>
    public Task<CfdiCancellationResponse> CancelAsync(
        string id,
        string? motive = null,
        string? uuidReplacement = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El id es obligatorio.", nameof(id));

        var queryParams = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(motive))
            queryParams["motive"] = motive;

        if (!string.IsNullOrWhiteSpace(uuidReplacement))
            queryParams["uuidReplacement"] = uuidReplacement;

        return _client.DeleteAsync<CfdiCancellationResponse>(
            $"{Resource}/{id}",
            queryParams,
            cancellationToken);
    }

    /// <summary>
    /// Envía una retención por correo electrónico.
    /// </summary>
    public Task<CfdiSendResponse> SendByEmailAsync(
        string id,
        string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El id es obligatorio.", nameof(id));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email es obligatorio.", nameof(email));

        var queryParams = new Dictionary<string, string?>
        {
            ["id"] = id,
            ["email"] = email
        };
        return _client.PostAsync<CfdiSendResponse>(
            $"{Resource}/envia",
            null!,
            queryParams,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Descarga una retención (PDF/XML) por ID.
    /// </summary>
    public Task<CfdiDownloadResponse> DownloadAsync(
        string fileType,
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El id es obligatorio.", nameof(id));
        if (string.IsNullOrWhiteSpace(fileType))
            throw new ArgumentException("El tipo de archivo es obligatorio.", nameof(fileType));

        return _client.GetAsync<CfdiDownloadResponse>(
            $"{Resource}/{id}/{fileType}",
            cancellationToken: cancellationToken);
    }
}
