namespace FacturamaNetSDK.Exceptions;

/// <summary>
/// Error de conectividad. No se pudo establecer conexión con la API.
/// </summary>
public sealed class FacturamaConnectionException : FacturamaException
{
    public FacturamaConnectionException()
        : base("No se pudo conectar con la API de Facturama. Verifica tu conexión a internet.") { }

    public FacturamaConnectionException(Exception innerException)
        : base("No se pudo conectar con la API de Facturama. Verifica tu conexión a internet.", innerException) { }
}