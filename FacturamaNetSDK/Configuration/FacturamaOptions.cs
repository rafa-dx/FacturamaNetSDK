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
    /// URL base explícita para escenarios de prueba local (mock server, WireMock, contenedor).
    /// Cuando se especifica, <see cref="Environment"/> se ignora.
    /// <para>
    /// Debe ser una URL absoluta con esquema http o https. Se admite <c>http://</c> únicamente
    /// en loopback: Basic Auth viaja en base64, así que apuntar a un host remoto sin TLS
    /// expondría las credenciales de la cuenta.
    /// </para>
    /// </summary>
    public Uri? BaseUrlOverride { get; set; }

    /// <summary>
    /// URL base efectiva, sin diagonal final: <see cref="BaseUrlOverride"/> si está presente,
    /// o la que corresponda al <see cref="Environment"/> configurado.
    /// </summary>
    internal string BaseUrl =>
        BaseUrlOverride?.AbsoluteUri.TrimEnd('/') ?? EnvironmentBaseUrl;

    private string EnvironmentBaseUrl => Environment == FacturamaEnvironment.Production
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
    /// Cuando faltan credenciales, <see cref="BaseUrlOverride"/> no es una URL http/https
    /// absoluta y segura, o el umbral del breaker no tolera una operación completa.
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

        ValidateBaseUrlOverride();

        Retry.Validate();
        CircuitBreaker.Validate();
        ValidateBreakerToleratesOneOperation();
    }

    /// <summary>
    /// <see cref="BaseUrlOverride"/> es la única opción capaz de desviar el tráfico —y con él las
    /// credenciales— a un host arbitrario, así que se valida al construir el cliente y no en la
    /// fábrica: un error de configuración no debe aparecer recién en la primera petición.
    /// </summary>
    private void ValidateBaseUrlOverride()
    {
        if (BaseUrlOverride is null)
            return;

        RequireAbsoluteUrl(BaseUrlOverride);
        RequireHttpScheme(BaseUrlOverride);
        RequireTlsOutsideLoopback(BaseUrlOverride);
    }

    private static void RequireAbsoluteUrl(Uri url)
    {
        if (url.IsAbsoluteUri)
            return;

        throw new ArgumentException(
            $"'{url}' no es una URL absoluta. Incluye el esquema y el host, " +
            "p.ej. http://localhost:5000.",
            nameof(BaseUrlOverride));
    }

    private static void RequireHttpScheme(Uri url)
    {
        if (url.Scheme is "http" or "https")
            return;

        throw new ArgumentException(
            $"El esquema '{url.Scheme}' no está soportado. Usa http o https.",
            nameof(BaseUrlOverride));
    }

    /// <summary>
    /// Basic Auth envía usuario y contraseña en base64 —codificados, no cifrados—, así que
    /// http sin TLS solo es aceptable contra la propia máquina.
    /// </summary>
    private static void RequireTlsOutsideLoopback(Uri url)
    {
        if (url.Scheme == "https" || url.IsLoopback)
            return;

        throw new ArgumentException(
            $"'{url}' usa http:// contra un host remoto. Basic Auth enviaría las credenciales " +
            "en base64 sin cifrar; usa https:// o un host de loopback " +
            "(localhost, 127.0.0.1, [::1]).",
            nameof(BaseUrlOverride));
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