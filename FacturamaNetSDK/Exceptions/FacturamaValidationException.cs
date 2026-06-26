namespace FacturamaNetSDK.Exceptions;

/// <summary>
/// Error de validación en la petición (400/422).
/// </summary>
public sealed class FacturamaValidationException : FacturamaException
{
    /// <summary>
    /// Errores de validación devueltos por la API, organizados por campo.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public FacturamaValidationException(string message)
        : base(message, 422)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public FacturamaValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message, 422)
    {
        Errors = errors;
    }
}