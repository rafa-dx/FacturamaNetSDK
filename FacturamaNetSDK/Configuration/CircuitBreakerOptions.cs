namespace FacturamaNetSDK.Configuration;

/// <summary>
/// Configuración del circuit breaker: corta el tráfico hacia la API cuando esta se degrada,
/// para dejar de castigar un servicio que ya está en problemas.
/// </summary>
/// <remarks>
/// <para>
/// El breaker abre tras una <b>racha</b> de operaciones fallidas consecutivas
/// (<see cref="FailuresBeforeBreaking"/>). Todos los umbrales cuentan <b>operaciones</b> —una
/// llamada del consumidor— y no intentos individuales: el breaker envuelve a la política de
/// reintentos, así que una operación que agota sus 4 intentos registra un solo fallo.
/// </para>
/// <para>
/// <b>Punto ciego conocido:</b> la racha se reinicia con un solo éxito intercalado, así que una
/// API con degradación <i>parcial</i> —que alterna fallos y respuestas correctas— no abre el
/// circuito. Cubrir ese caso requiere una política por proporción de fallos sobre una ventana
/// deslizante, que exige un volumen sostenido de peticiones que un consumidor de facturación
/// típico no alcanza. Se evaluará para una versión futura si aparece el caso de uso
/// (timbrado masivo concurrente).
/// </para>
/// <para>
/// El breaker es único por instancia de <c>FacturamaClient</c>: todos los endpoints comparten su
/// estado, de modo que la protección aplica a la cuenta completa y no a cada ruta por separado.
/// </para>
/// </remarks>
public sealed record CircuitBreakerOptions
{
    /// <summary>Activa o desactiva el circuit breaker. Default: true.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Tiempo que el circuito permanece abierto antes de pasar a half-open. Default: 30s.
    /// <para>
    /// ⚠️ <b>A definir con el equipo</b> según el tiempo de recuperación real de la API de
    /// Facturama. Mientras está abierto, toda petición falla de inmediato con
    /// <c>FacturamaServerException</c> (503) sin tocar la red.
    /// </para>
    /// </summary>
    public TimeSpan BreakDuration { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Operaciones fallidas <b>consecutivas</b> que abren el circuito. Default: 5.
    /// <para>
    /// Cuenta operaciones completas: una llamada que agota sus reintentos suma 1, no uno por
    /// intento. El tiempo que tarda en abrir depende por completo de <i>cómo</i> falle la API,
    /// y el rango es amplio (cifras con los defaults de reintentos y timeout):
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>API caída, respondiendo 5xx al instante:</b> cada operación gasta solo su backoff
    ///     (~14s), así que 5 operaciones abren el circuito en <b>~1 minuto</b>.
    ///   </description></item>
    ///   <item><description>
    ///     <b>API colgada, agotando el timeout de cada intento:</b> cada operación gasta ~54s,
    ///     así que abrir tarda <b>~4.5 minutos</b>.
    ///   </description></item>
    /// </list>
    /// <para>
    /// ⚠️ <b>A definir con el equipo.</b> El segundo caso es el peor de los dos —el consumidor
    /// se cuelga minutos antes de quedar protegido— y es justo donde el breaker más se necesita.
    /// Si ese escenario importa, la palanca es bajar <c>FacturamaOptions.Timeout</c>, no este
    /// umbral: el umbral cuenta operaciones, no tiempo.
    /// </para>
    /// </summary>
    public int FailuresBeforeBreaking { get; init; } = 5;

    /// <summary>
    /// Valida la coherencia interna de estas opciones.
    /// </summary>
    internal void Validate()
    {
        if (!Enabled)
            return;

        if (FailuresBeforeBreaking < 2)
            throw new ArgumentOutOfRangeException(
                nameof(FailuresBeforeBreaking),
                FailuresBeforeBreaking,
                "Debe ser al menos 2. Con 1, una única operación fallida abre el circuito.");

        if (BreakDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(BreakDuration),
                BreakDuration,
                "Debe ser mayor a cero.");
    }
}
