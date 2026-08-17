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

    /// <summary>
    /// Política de reintentos ante errores transitorios.
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Política del circuit breaker, compartida por todos los endpoints del cliente.
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Valida que la configuración sea correcta.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Cuando faltan credenciales o el umbral del breaker no tolera una operación completa.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Cuando el timeout, los reintentos o los umbrales del breaker están fuera de rango.
    /// </exception>
    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(Username))
            throw new ArgumentException("El Username es requerido.", nameof(Username));

        if (string.IsNullOrWhiteSpace(Password))
            throw new ArgumentException("El Password es requerido.", nameof(Password));

        if (Timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(Timeout), Timeout, "Debe ser mayor a cero.");

        Retry.Validate();
        CircuitBreaker.Validate();
        ValidateBreakerToleratesOneOperation();
    }

    /// <summary>
    /// Ambas capas del circuit breaker cuentan intentos, no operaciones: cada reintento pasa
    /// por ellas. Si un umbral no supera los intentos de una sola operación, una petición
    /// aislada deja el circuito abierto y la siguiente llamada —aunque sea a otro endpoint—
    /// falla con un 503 sin llegar a la red.
    /// </summary>
    private void ValidateBreakerToleratesOneOperation()
    {
        if (!CircuitBreaker.Enabled)
            return;

        var attempts = Retry.MaxAttemptsPerOperation;

        RequireAboveAttempts(
            CircuitBreaker.FailuresBeforeBreaking,
            attempts,
            nameof(CircuitBreakerOptions.FailuresBeforeBreaking));

        RequireAboveAttempts(
            CircuitBreaker.MinimumThroughput,
            attempts,
            nameof(CircuitBreakerOptions.MinimumThroughput));
    }

    private static void RequireAboveAttempts(int value, int attempts, string setting)
    {
        if (value > attempts)
            return;

        throw new ArgumentException(
            $"CircuitBreaker.{setting} ({value}) debe ser mayor que los intentos de una sola operación " +
            $"({attempts} = Retry.MaxRetries + 1). El breaker cuenta intentos, no operaciones: con este " +
            $"valor una única petición fallida deja el circuito abierto para toda la cuenta. " +
            $"Sugerencia: usa al menos {attempts + 1}.",
            nameof(CircuitBreaker));
    }
}