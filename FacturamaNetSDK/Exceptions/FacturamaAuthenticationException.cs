namespace FacturamaNetSDK.Exceptions;

public class FacturamaAuthenticationException : FacturamaException
    {
    public FacturamaAuthenticationException()
        : base("Credenciales inválidas. Verifica tu Username y Password.") { }

    public FacturamaAuthenticationException(string message)
        : base(message, 401) { }
    

}

