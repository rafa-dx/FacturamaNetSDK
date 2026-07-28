using FacturamaNetSDK.Models.SubscriptionPlan;

namespace FacturamaNetSDK.Endpoints.Abstractions;

/// <summary>
/// Operaciones del plan de suscripción de la cuenta.
/// </summary>
public interface ISubscriptionPlanEndpoint
{
    /// <summary>
    /// Obtiene la información del plan de suscripción.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El plan de suscripción vigente.</returns>
    Task<SubscriptionPlanResponse> GetAsync(CancellationToken cancellationToken = default);
}
