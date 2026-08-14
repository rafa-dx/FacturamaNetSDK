namespace FacturamaNetSDK.Exceptions;

/// <summary>
/// Credenciales inválidas o ausentes (401).
/// </summary>
public class FacturamaAuthenticationException : FacturamaException
{
    public FacturamaAuthenticationException()
        : base("Credenciales inválidas. Verifica tu Username y Password.", 401) { }

    public FacturamaAuthenticationException(string message)
        : base(message, 401) { }
}
