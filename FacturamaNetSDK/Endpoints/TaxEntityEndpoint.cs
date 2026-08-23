using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Internal;
using FacturamaNetSDK.Models.TaxEntity.Request;
using FacturamaNetSDK.Models.TaxEntity.Response;

namespace FacturamaNetSDK.Endpoints;

/// <summary>
/// Operaciones del perfil fiscal (contribuyente) de la cuenta.
/// </summary>
public sealed class TaxEntityEndpoint : ITaxEntityEndpoint
{
    private const string Resource = "taxentity";
    private readonly FacturamaHttpClient _client;

    internal TaxEntityEndpoint(FacturamaHttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc />
    public Task<TaxEntityResponse> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return _client.GetAsync<TaxEntityResponse>(
            Resource,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<TaxEntityResponse> UpdateInfoAsync(
        TaxEntityRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(request, nameof(request));
        return _client.PutAsync<TaxEntityResponse>(
            Resource,
            request,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ImageResponse> UpdateLogoAsync(
        TaxEntityLogoRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(request, nameof(request));
        return _client.PutAsync<ImageResponse>(
            $"{Resource}/UploadLogo",
            request,
            cancellationToken);
    }
}
