using FacturamaNetSDK.Models.BranchOffice.Request;
using FacturamaNetSDK.Models.BranchOffice.Response;

namespace FacturamaNetSDK.Endpoints.Abstractions;

/// <summary>
/// Operaciones de sucursales del perfil fiscal.
/// </summary>
public interface IBranchOfficeEndpoint
{
    /// <summary>Agrega una sucursal.</summary>
    /// <param name="request">Los datos de la sucursal a crear.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<BranchOfficeResponse> AddAsync(
        BranchOfficeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Actualiza una sucursal existente.</summary>
    /// <param name="branchOfficeId">Identificador de la sucursal.</param>
    /// <param name="request">Los datos de la sucursal a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<BranchOfficeResponse> UpdateAsync(
        string branchOfficeId,
        BranchOfficeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Lista todas las sucursales.</summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<IReadOnlyList<BranchOfficeResponse>> ListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene una sucursal por su identificador.</summary>
    /// <param name="branchOfficeId">Identificador de la sucursal.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<BranchOfficeResponse> GetAsync(
        string branchOfficeId,
        CancellationToken cancellationToken = default);

    /// <summary>Elimina una sucursal por su identificador.</summary>
    /// <param name="branchOfficeId">Identificador de la sucursal.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<BranchOfficeResponse> DeleteAsync(
        string branchOfficeId,
        CancellationToken cancellationToken = default);
}
