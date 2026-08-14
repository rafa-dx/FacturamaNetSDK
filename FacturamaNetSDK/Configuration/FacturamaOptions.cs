namespace FacturamaNetSDK.Configuration;

/// <summary>
/// Opciones de configuración para el cliente de Facturama.
/// </summary>
public sealed class FacturamaOptions
{
    /// <summary>
    /// Nombre de usuario para autenticación.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña para autenticación.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Ambiente de la API. Default: Sandbox.
    /// </summary>
    public FacturamaEnvironment Environment { get; set; } = FacturamaEnvironment.Sandbox;

    /// <summary>
    /// Versión de la API Lite. Default: V3.
    /// </summary>
    public ApiLiteVersion ApiLiteVersion { get; set; } = ApiLiteVersion.V3;

    /// <summary>
    /// Timeout para las peticiones HTTP. Default: 30 segundos.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Obtiene la URL base según el ambiente configurado.
    /// </summary>
    internal string BaseUrl => Environment == FacturamaEnvironment.Production
        ? "https://api.facturama.mx"
        : "https://apisandbox.facturama.mx";

    public RetryOptions Retry { get; set; } = new();
    // public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Valida que la configuración sea correcta.
    /// </summary>
    /// <exception cref="ArgumentException">Cuando Username o Password están vacíos.</exception>
    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(Username))
            throw new ArgumentException("El Username es requerido.", nameof(Username));

        if (string.IsNullOrWhiteSpace(Password))
            throw new ArgumentException("El Password es requerido.", nameof(Password));
    }
}