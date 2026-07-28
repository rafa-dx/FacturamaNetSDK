using FacturamaNetSDK.Models.Series.Request;
using FacturamaNetSDK.Models.Series.Response;

namespace FacturamaNetSDK.Endpoints.Abstractions;

/// <summary>
/// Operaciones de series de folios, asociadas a una sucursal.
/// </summary>
public interface ISeriesEndpoint
{
    /// <summary>Lista las series de una sucursal.</summary>
    /// <param name="branchOfficeId">Identificador de la sucursal.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<IReadOnlyList<SerieResponse>> ListAsync(
        string branchOfficeId,
        CancellationToken cancellationToken = default);

    /// <summary>Agrega una serie a una sucursal.</summary>
    /// <param name="branchOfficeId">Identificador de la sucursal.</param>
    /// <param name="request">Los datos de la serie a crear.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<SerieResponse> AddAsync(
        string branchOfficeId,
        SerieRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Elimina una serie de una sucursal por su nombre.</summary>
    /// <param name="branchOfficeId">Identificador de la sucursal.</param>
    /// <param name="serieName">Nombre de la serie a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<SerieResponse> DeleteAsync(
        string branchOfficeId,
        string serieName,
        CancellationToken cancellationToken = default);
}
