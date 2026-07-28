using FacturamaNetSDK.Models.TaxEntity.Request;
using FacturamaNetSDK.Models.TaxEntity.Response;

namespace FacturamaNetSDK.Endpoints.Abstractions;

/// <summary>
/// Operaciones del perfil fiscal (contribuyente) de la cuenta.
/// </summary>
public interface ITaxEntityEndpoint
{
    /// <summary>Obtiene la información del perfil fiscal.</summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<TaxEntityResponse> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Actualiza la información del perfil fiscal.</summary>
    /// <param name="request">La información del perfil fiscal a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<TaxEntityResponse> UpdateInfoAsync(
        TaxEntityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Actualiza el logo del perfil fiscal.</summary>
    /// <param name="request">La información del logo a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<ImageResponse> UpdateLogoAsync(
        TaxEntityLogoRequest request,
        CancellationToken cancellationToken = default);
}
