namespace FacturamaNetSDK.Models.SubscriptionPlan;

/// <summary>
/// Información del plan de suscripción de la cuenta.
/// </summary>
public sealed record SubscriptionPlanResponse
{
    /// <summary>Nombre del plan contratado.</summary>
    public string Plan { get; init; } = string.Empty;

    /// <summary>Folios disponibles en el periodo actual.</summary>
    public string CurrentFolios { get; init; } = string.Empty;

    /// <summary>Fecha de creación del plan.</summary>
    public string CreationDate { get; init; } = string.Empty;

    /// <summary>Fecha de expiración del plan.</summary>
    public string ExpirationDate { get; init; } = string.Empty;
}
