namespace FacturamaNetSDK.Exceptions;

/// <summary>
/// Error interno del servidor de Facturama (5xx).
/// </summary>
public sealed class FacturamaServerException : FacturamaException
{
    public FacturamaServerException(int statusCode)
        : base("Error interno en el servidor de Facturama. Intenta nuevamente más tarde.", statusCode) { }

    public FacturamaServerException(string message, int statusCode)
        : base(message, statusCode) { }
}