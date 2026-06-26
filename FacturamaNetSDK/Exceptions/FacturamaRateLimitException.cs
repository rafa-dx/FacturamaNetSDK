namespace FacturamaNetSDK.Exceptions;

/// <summary>
/// Límite de peticiones excedido (429).
/// </summary>
public sealed class FacturamaRateLimitException : FacturamaException
{
    /// <summary>
    /// Tiempo de espera sugerido antes de reintentar.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    public FacturamaRateLimitException(TimeSpan? retryAfter = null)
        : base("Límite de peticiones excedido. Intenta nuevamente más tarde.", 429)
    {
        RetryAfter = retryAfter;
    }
}