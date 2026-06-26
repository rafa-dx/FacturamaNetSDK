using FacturamaNetSDK.Models.Client.Request;
using FacturamaNetSDK.Models.Client.Response;

namespace FacturamaNetSDK.Endpoints.Abstractions;

/// <summary>
/// Operaciones de clientes.
/// </summary>
public interface IClientEndpoint
{
    /// <summary>Crea un nuevo cliente.</summary>
    Task<ClientResponse> CreateAsync(ClientRequest request, CancellationToken cancellationToken = default);

    /// <summary>Obtiene un cliente por ID.</summary>
    Task<ClientResponse> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Lista clientes con keyword opcional.</summary>
    Task<IReadOnlyList<ClientResponse>> ListAsync(string? keyword = null, CancellationToken cancellationToken = default);

    /// <summary>Actualiza un cliente existente.</summary>
    Task UpdateAsync(string id, ClientRequest request, CancellationToken cancellationToken = default);

    /// <summary>Elimina un cliente por ID.</summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Consulta el status de un RFC en el SAT.</summary>
    Task<RfcStatusResponse> GetRfcStatusAsync(string rfc, CancellationToken cancellationToken = default);

    /// <summary>Valida los datos fiscales de un receptor contra el SAT.</summary>
    Task<CustomerValidationResponse> ValidateAsync(CustomerValidationRequest request, CancellationToken cancellationToken = default);
}
