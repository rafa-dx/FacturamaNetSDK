namespace FacturamaNetSDK.Exceptions;

/// <summary>
/// Recurso no encontrado (404).
/// </summary>
public sealed class FacturamaNotFoundException : FacturamaException
{
    /// <summary>
    /// Identificador del recurso que no fue encontrado.
    /// </summary>
    public string? ResourceId { get; }

    public FacturamaNotFoundException(string resourceId)
        : base($"El recurso '{resourceId}' no fue encontrado.", 404)
    {
        ResourceId = resourceId;
    }

    public FacturamaNotFoundException(string resourceId, string message)
        : base(message, 404)
    {
        ResourceId = resourceId;
    }
}