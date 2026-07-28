using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Series.Request;
using FacturamaNetSDK.Models.Series.Response;

namespace FacturamaNetSDK.Endpoints;

/// <summary>
/// Operaciones de series de folios, asociadas a una sucursal.
/// </summary>
public sealed class SeriesEndpoint : ISeriesEndpoint
{
    private const string Resource = "Serie";
    private readonly FacturamaHttpClient _client;

    internal SeriesEndpoint(FacturamaHttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SerieResponse>> ListAsync(
        string branchOfficeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(branchOfficeId))
            throw new ArgumentNullException(nameof(branchOfficeId));
        var result = await _client.GetAsync<List<SerieResponse>>(
            $"{Resource}/{branchOfficeId}",
            cancellationToken: cancellationToken);
        return result is null ? Array.Empty<SerieResponse>() : result.AsReadOnly();
    }

    /// <inheritdoc />
    public Task<SerieResponse> AddAsync(
        string branchOfficeId,
        SerieRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(branchOfficeId))
            throw new ArgumentNullException(nameof(branchOfficeId));
        ArgumentNullException.ThrowIfNull(request);
        return _client.PostAsync<SerieResponse>(
            $"{Resource}/{branchOfficeId}",
            request,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<SerieResponse> DeleteAsync(
        string branchOfficeId,
        string serieName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(branchOfficeId))
            throw new ArgumentNullException(nameof(branchOfficeId));
        if (string.IsNullOrWhiteSpace(serieName))
            throw new ArgumentNullException(nameof(serieName));
        return _client.DeleteAsync<SerieResponse>(
            $"{Resource}/{branchOfficeId}/{serieName}",
            cancellationToken: cancellationToken);
    }
}
