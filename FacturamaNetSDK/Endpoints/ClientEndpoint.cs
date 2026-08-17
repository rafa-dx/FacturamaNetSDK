using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.Client;
using FacturamaNetSDK.Models.Client.Request;
using FacturamaNetSDK.Models.Client.Response;
using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Utilities;
using System.Runtime.CompilerServices;

namespace FacturamaNetSDK.Endpoints;

/// <summary>
/// Operaciones de clientes (/Client, /customers).
/// </summary>
public sealed class ClientEndpoint : IClientEndpoint
{
    private const string Resource = "Client";
    private const string CustomersResource = "customers";
    private readonly FacturamaHttpClient _client;

    internal ClientEndpoint(FacturamaHttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Crea un nuevo cliente.
    /// </summary>
    public Task<ClientResponse> CreateAsync(
        ClientRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.PostAsync<ClientResponse>(
            Resource, 
            request, 
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Elimina un cliente por ID.
    /// </summary>
    public Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        EnsureId(id);
        return _client.DeleteAsync<ClientResponse>(
            $"{Resource}/{id}", 
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Obtiene un cliente por ID.
    /// </summary>
    public Task<ClientResponse> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        EnsureId(id);
        return _client.GetAsync<ClientResponse>(
            $"{Resource}/{id}", 
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Lista clientes con keyword opcional.
    /// </summary>
    public async Task<IReadOnlyList<ClientResponse>> ListAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = keyword is not null
            ? new Dictionary<string, string?> { ["keyword"] = keyword }
            : null;

        var result = await _client.GetAsync<List<ClientResponse>>(
            Resource, queryParams, cancellationToken).ConfigureAwait(false);
        return result is null ? Array.Empty<ClientResponse>() : result.AsReadOnly();
    }

    public Task<FilterClientResponse> ListAsync(
        QueryOptions? filters = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = filters is not null
            ? QueryBuilder.FromObject(filters)
            : null;
        return _client.GetAsync<FilterClientResponse>(
            Resource, 
            queryParams, 
            cancellationToken);
    }

    /// <summary>
    /// Actualiza un cliente existente.
    /// </summary>
    public Task UpdateAsync(
        string id,
        ClientRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureId(id);
        ArgumentNullException.ThrowIfNull(request);
        return _client.PutAsync<object>(
            $"{Resource}/{id}", 
            request, 
            cancellationToken);
    }

    /// <summary>
    /// Consulta el status de un RFC en el SAT.
    /// </summary>
    public Task<RfcStatusResponse> GetRfcStatusAsync(
        string rfc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rfc))
            throw new ArgumentNullException(nameof(rfc));
        return _client.GetAsync<RfcStatusResponse>(
            $"{Resource}/status",
            queryParams: new() { ["rfc"] = rfc },
            cancellationToken);
    }

    /// <summary>
    /// Valida los datos fiscales de un receptor contra el SAT.
    /// </summary>
    public Task<CustomerValidationResponse> ValidateAsync(
        CustomerValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _client.PostAsync<CustomerValidationResponse>(
            $"{CustomersResource}/validate", request, cancellationToken: cancellationToken);
    }

    private static void EnsureId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
    }
}