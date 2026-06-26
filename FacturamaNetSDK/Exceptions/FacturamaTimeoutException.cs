namespace FacturamaNetSDK.Exceptions;

/// <summary>
/// La petición excedió el tiempo de espera configurado.
/// </summary>
public sealed class FacturamaTimeoutException : FacturamaException
{
    public FacturamaTimeoutException()
        : base("La petición a Facturama excedió el tiempo de espera.") { }

    public FacturamaTimeoutException(Exception innerException)
        : base("La petición a Facturama excedió el tiempo de espera.", innerException) { }
}