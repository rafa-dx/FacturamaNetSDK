namespace FacturamaNetSDK.Configuration;

/// <summary>
/// Configuración del circuit breaker: corta el tráfico hacia la API cuando esta se degrada,
/// para dejar de castigar un servicio que ya está en problemas.
/// </summary>
/// <remarks>
/// <para>
/// Son <b>dos capas encadenadas</b> que cubren los puntos ciegos de la otra. Cualquiera de las
/// dos puede abrir el circuito, y ambas comparten <see cref="BreakDuration"/>:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Racha</b> (<see cref="FailuresBeforeBreaking"/>): abre tras N fallos <i>consecutivos</i>.
///     Protege al consumidor de bajo volumen y detecta una caída total de la API. Ciega a la
///     degradación parcial: un solo éxito intercalado reinicia su contador.
///   </description></item>
///   <item><description>
///     <b>Ratio</b> (<see cref="FailureRatio"/>, <see cref="SamplingDuration"/>,
///     <see cref="MinimumThroughput"/>): abre cuando la <i>proporción</i> de fallos en una ventana
///     deslizante supera el umbral. Detecta degradación parcial bajo carga. Ciega al volumen bajo:
///     por debajo de <see cref="MinimumThroughput"/> nunca actúa.
///   </description></item>
/// </list>
/// <para>
/// El breaker es único por instancia de <c>FacturamaClient</c>: todos los endpoints comparten su
/// estado, de modo que la protección aplica a la cuenta completa y no a cada ruta por separado.
/// </para>
/// </remarks>
public sealed record CircuitBreakerOptions
{
    /// <summary>
    /// Ventana de muestreo mínima admitida por el temporizador interno de Polly.
    /// </summary>
    public static readonly TimeSpan MinimumSamplingDuration = TimeSpan.FromMilliseconds(20);

    /// <summary>Activa o desactiva ambas capas del circuit breaker. Default: true.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Tiempo que el circuito permanece abierto antes de pasar a half-open. Compartido por
    /// las dos capas. Default: 30s.
    /// <para>
    /// ⚠️ <b>A definir con el equipo</b> según el tiempo de recuperación real de la API de
    /// Facturama. Mientras está abierto, toda petición falla de inmediato con
    /// <c>FacturamaServerException</c> (503) sin tocar la red.
    /// </para>
    /// </summary>
    public TimeSpan BreakDuration { get; init; } = TimeSpan.FromSeconds(30);

    // --- Capa 1: racha de fallos consecutivos ---

    /// <summary>
    /// Fallos <b>consecutivos</b> que abren el circuito. Default: 10.
    /// <para>
    /// ⚠️ <b>A definir con el equipo.</b> Cuenta <b>intentos</b>, no operaciones: cada reintento
    /// pasa por el breaker. Con <see cref="RetryOptions.MaxRetries"/> = 3 (4 intentos por
    /// operación), el default de 10 tolera 2 operaciones completas fallidas antes de abrir.
    /// Para tolerar N operaciones, usar <c>N * (MaxRetries + 1) + 1</c>.
    /// </para>
    /// </summary>
    public int FailuresBeforeBreaking { get; init; } = 10;

    // --- Capa 2: proporción de fallos en ventana deslizante ---

    /// <summary>
    /// Proporción de fallos que abre el circuito, entre 0 (exclusivo) y 1 (inclusivo).
    /// Default: 0.5 (50%).
    /// <para>
    /// ⚠️ <b>A definir con el equipo.</b> Solo se evalúa al registrarse un fallo, y únicamente
    /// si la ventana acumuló al menos <see cref="MinimumThroughput"/> peticiones.
    /// </para>
    /// </summary>
    public double FailureRatio { get; init; } = 0.5;

    /// <summary>
    /// Ventana deslizante sobre la que se calcula <see cref="FailureRatio"/>. Default: 60s.
    /// <para>
    /// ⚠️ <b>A definir con el equipo.</b> Los fallos anteriores a esta ventana se olvidan.
    /// Alargarla ayuda a los consumidores de bajo volumen a alcanzar
    /// <see cref="MinimumThroughput"/>, a costa de reaccionar más lento.
    /// </para>
    /// </summary>
    public TimeSpan SamplingDuration { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Peticiones mínimas en la ventana para que <see cref="FailureRatio"/> se considere
    /// significativo. Default: 20.
    /// <para>
    /// ⚠️ <b>A definir con el equipo.</b> Por debajo de este volumen esta capa <b>nunca</b> abre
    /// el circuito, sin importar cuántos fallos haya; de eso se encarga la capa de racha.
    /// Al igual que <see cref="FailuresBeforeBreaking"/>, cuenta intentos y no operaciones.
    /// </para>
    /// </summary>
    public int MinimumThroughput { get; init; } = 20;

    /// <summary>
    /// Valida la coherencia interna de estas opciones. La relación con
    /// <see cref="RetryOptions"/> se valida en <see cref="FacturamaOptions.Validate"/>,
    /// que es quien conoce ambas configuraciones.
    /// </summary>
    internal void Validate()
    {
        if (!Enabled)
            return;

        ValidateConsecutiveFailureLayer();
        ValidateFailureRatioLayer();

        if (BreakDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(BreakDuration),
                BreakDuration,
                "Debe ser mayor a cero.");
    }

    private void ValidateConsecutiveFailureLayer()
    {
        if (FailuresBeforeBreaking < 2)
            throw new ArgumentOutOfRangeException(
                nameof(FailuresBeforeBreaking),
                FailuresBeforeBreaking,
                "Debe ser al menos 2. Con 1 fallo, cualquier error aislado abre el circuito.");
    }

    private void ValidateFailureRatioLayer()
    {
        // Negado a propósito: con NaN toda comparación directa es falsa y se colaría.
        if (!(FailureRatio > 0 && FailureRatio <= 1))
            throw new ArgumentOutOfRangeException(
                nameof(FailureRatio),
                FailureRatio,
                "Debe ser mayor a 0 y menor o igual a 1 (0.5 = 50% de las peticiones).");

        if (SamplingDuration < MinimumSamplingDuration)
            throw new ArgumentOutOfRangeException(
                nameof(SamplingDuration),
                SamplingDuration,
                $"Debe ser de al menos {MinimumSamplingDuration.TotalMilliseconds}ms, la resolución del temporizador del breaker.");

        if (MinimumThroughput < 2)
            throw new ArgumentOutOfRangeException(
                nameof(MinimumThroughput),
                MinimumThroughput,
                "Debe ser al menos 2. Con 1, una única petición fallida abre el circuito.");
    }
}
