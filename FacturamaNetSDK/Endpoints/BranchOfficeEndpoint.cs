using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.BranchOffice.Request;
using FacturamaNetSDK.Models.BranchOffice.Response;

namespace FacturamaNetSDK.Endpoints;

/// <summary>
/// Operaciones de sucursales del perfil fiscal.
/// </summary>
public sealed class BranchOfficeEndpoint : IBranchOfficeEndpoint
{
    private const string Resource = "BranchOffice";
    private readonly FacturamaHttpClient _client;

    internal BranchOfficeEndpoint(FacturamaHttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc />
    public Task<BranchOfficeResponse> AddAsync(
        BranchOfficeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.PostAsync<BranchOfficeResponse>(
            Resource,
            request,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<BranchOfficeResponse> UpdateAsync(
        string branchOfficeId,
        BranchOfficeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(branchOfficeId))
            throw new ArgumentNullException(nameof(branchOfficeId));
        ArgumentNullException.ThrowIfNull(request);
        return _client.PutAsync<BranchOfficeResponse>(
            $"{Resource}/{branchOfficeId}",
            request,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BranchOfficeResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _client.GetAsync<List<BranchOfficeResponse>>(
            Resource,
            cancellationToken: cancellationToken);
        return result is null ? Array.Empty<BranchOfficeResponse>() : result.AsReadOnly();
    }

    /// <inheritdoc />
    public Task<BranchOfficeResponse> GetAsync(
        string branchOfficeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(branchOfficeId))
            throw new ArgumentNullException(nameof(branchOfficeId));
        return _client.GetAsync<BranchOfficeResponse>(
            $"{Resource}/{branchOfficeId}",
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<BranchOfficeResponse> DeleteAsync(
        string branchOfficeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(branchOfficeId))
            throw new ArgumentNullException(nameof(branchOfficeId));
        return _client.DeleteAsync<BranchOfficeResponse>(
            $"{Resource}/{branchOfficeId}",
            cancellationToken: cancellationToken);
    }
}
