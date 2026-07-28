using FacturamaNetSDK.Endpoints.Abstractions;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Models.SubscriptionPlan;

namespace FacturamaNetSDK.Endpoints;

/// <summary>
/// Operaciones del plan de suscripción de la cuenta.
/// </summary>
public sealed class SubscriptionPlanEndpoint : ISubscriptionPlanEndpoint
{
    // ⚠️ a verificar con el equipo/API: ruta real del recurso de suscripción
    // (se conserva el valor original; solo se corrigió la grafía del identificador C#).
    private const string Resource = "suscriptionplan";
    private readonly FacturamaHttpClient _client;

    internal SubscriptionPlanEndpoint(FacturamaHttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc />
    public Task<SubscriptionPlanResponse> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return _client.GetAsync<SubscriptionPlanResponse>(
            Resource,
            cancellationToken: cancellationToken);
    }
}
