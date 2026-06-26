

namespace FacturamaNetSDK.Exceptions;

/// <summary>
/// Excepción base para todos los errores del SDK de Facturama.
/// </summary>
public class FacturamaException : Exception
{
    /// <summary>
    /// Código de estado HTTP asociado al error.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Identificador de la petición, útil para soporte y diagnóstico.
    /// </summary>
    public string? RequestId { get; }

    public FacturamaException(string message)
    : base(message) { }
    public FacturamaException(string message, Exception innerException)
        : base(message, innerException) { }


    public FacturamaException(string message, int statusCode, string? requestId = null)
        : base(message)
    {
        StatusCode = statusCode;
        RequestId = requestId;
    }
}

