using FacturamaNetSDK.Authentication;
using FacturamaNetSDK.Configuration;
using FacturamaNetSDK.Internal;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace FacturamaNetSDK.Http;

/// <summary>
/// Construye instancias configuradas de <see cref="FacturamaHttpClient"/>.
/// Todos los clientes creados por la misma instancia comparten el circuit breaker,
/// de modo que la protección aplica a la cuenta completa y no a cada ruta por separado.
/// </summary>
internal sealed class FacturamaHttpClientFactory
{
    private static readonly TimeSpan SafetyMargin = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 429 no existe en el enum <see cref="System.Net.HttpStatusCode"/> de netstandard2.0.
    /// </summary>
    private const int TooManyRequests = 429;

    /// <summary>
    /// Fuente de aleatoriedad por defecto para el jitter. Una instancia por hilo:
    /// <c>Random</c> no es thread-safe y <c>Random.Shared</c> es net6+.
    /// </summary>
    private static readonly ThreadLocal<Random> DefaultRandom =
        new(() => new Random(Guid.NewGuid().GetHashCode()));

    private readonly FacturamaOptions _options;
    private readonly ILogger? _logger;
    private readonly Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> _policySelector;
    private readonly TimeSpan _totalBudget;

    /// <param name="options">Configuración del cliente.</param>
    /// <param name="logger">Logger opcional.</param>
    /// <param name="randomSample">
    /// Muestra aleatoria en [0,1) para el jitter del backoff. Inyectable para fijarla en pruebas.
    /// </param>
    /// <param name="utcNow">
    /// Reloj usado para resolver un <c>Retry-After</c> expresado como fecha absoluta.
    /// </param>
    internal FacturamaHttpClientFactory(
        FacturamaOptions options,
        ILogger? logger = null,
        Func<double>? randomSample = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _policySelector = BuildPolicySelector(
            options, logger ?? NullLogger.Instance, randomSample, utcNow);
        _totalBudget = CalculateTotalBudget(options);
    }

    /// <summary>
    /// Cliente para rutas raíz: /Client, /Product, /catalogs, /customers
    /// </summary>
    internal FacturamaHttpClient CreateRootClient() => Create(pathPrefix: null);

    /// <summary>
    /// Cliente para rutas versionadas: /api/3/cfdis, /api/2/retenciones, /api-lite/{version}/cfdis
    /// </summary>
    internal FacturamaHttpClient CreateApiClient(string pathPrefix) => Create(pathPrefix);

    // -------------------------------------------------------------------------

    private FacturamaHttpClient Create(string? pathPrefix)
    {
        var authHandler = new BasicAuthenticationHandler(_options.Username, _options.Password)
        {
            InnerHandler = new PolicyHttpMessageHandler(_policySelector)
            {
                InnerHandler = new HttpClientHandler()
            }
        };

        DelegatingHandler pipeline = _logger is not null
            ? new LoggingHandler(_logger) { InnerHandler = authHandler }
            : authHandler;

        var httpClient = new HttpClient(pipeline)
        {
            BaseAddress = new Uri(BuildBaseUrl(pathPrefix)),
            // Techo total de la operación completa. El timeout por intento lo aplica Polly:
            // si este valor fuera igual a options.Timeout, cortaría los reintentos a medias.
            Timeout = _totalBudget
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(SdkVersion.UserAgent);

        return new FacturamaHttpClient(httpClient);
    }

    private string BuildBaseUrl(string? pathPrefix) =>
        string.IsNullOrEmpty(pathPrefix)
            ? $"{_options.BaseUrl}/"
            : $"{_options.BaseUrl}/{pathPrefix}/";

    // -------------------------------------------------------------------------
    // Políticas de resiliencia
    // -------------------------------------------------------------------------

    /// <summary>
    /// Construye las políticas una sola vez y devuelve el selector por petición. El breaker
    /// resultante es una única instancia compartida por todos los clientes de esta fábrica.
    /// <para>
    /// El breaker envuelve al retry, no al revés: así registra <b>una operación</b> por cada
    /// llamada del consumidor —no un evento por reintento— y sus umbrales se leen en la misma
    /// unidad que usa quien los configura. Con el circuito abierto la petición falla antes de
    /// entrar al retry, sin consumir intentos ni esperas de backoff.
    /// </para>
    /// </summary>
    internal static Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> BuildPolicySelector(
        FacturamaOptions options,
        ILogger log,
        Func<double>? randomSample = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        // El breaker y el timeout por intento siempre se construyen: protegen el servicio
        // con independencia de que los reintentos estén activos o no.
        var perAttemptTimeout = BuildPerAttemptTimeout(options, log);
        var circuitBreaker = BuildCircuitBreaker(options, log);
        var withoutRetry = Policy.WrapAsync(circuitBreaker, perAttemptTimeout);

        if (!options.Retry.Enabled)
            return _ => withoutRetry;

        var retry = BuildRetry(
            options,
            log,
            randomSample ?? (() => DefaultRandom.Value!.NextDouble()),
            utcNow ?? (() => DateTimeOffset.UtcNow));

        var withRetry = Policy.WrapAsync(circuitBreaker, retry, perAttemptTimeout);

        // Solo reintentan los verbos habilitados; el resto usa breaker + timeout.
        return request => options.Retry.ShouldRetry(request.Method) ? withRetry : withoutRetry;
    }

    private static IAsyncPolicy<HttpResponseMessage> BuildPerAttemptTimeout(
        FacturamaOptions options,
        ILogger log) =>
        Policy.TimeoutAsync<HttpResponseMessage>(
            options.Timeout,
            TimeoutStrategy.Optimistic,
            onTimeoutAsync: (_, timespan, _, _) =>
            {
                log.LogWarning("Intento abortado por timeout de {Seconds}s", timespan.TotalSeconds);
                return Task.CompletedTask;
            });

    /// <summary>
    /// Breaker por racha de operaciones fallidas consecutivas. Cuando está deshabilitado se
    /// devuelve una política inerte, para que el resto del wrap no tenga que ramificar.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> BuildCircuitBreaker(
        FacturamaOptions options,
        ILogger log)
    {
        var breaker = options.CircuitBreaker;

        if (!breaker.Enabled)
            return Policy.NoOpAsync<HttpResponseMessage>();

        return HandledFailures().CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: breaker.FailuresBeforeBreaking,
            durationOfBreak: breaker.BreakDuration,
            onBreak: (outcome, duration) =>
                log.LogError(
                    "Circuit breaker abierto por {Seconds}s — {Failures} fallos consecutivos. Último: {Reason}",
                    duration.TotalSeconds,
                    breaker.FailuresBeforeBreaking,
                    Describe(outcome)),
            onReset: () =>
                log.LogInformation("Circuit breaker cerrado — reanudando peticiones"),
            onHalfOpen: () =>
                log.LogInformation("Circuit breaker en half-open — probando conexión"));
    }

    /// <summary>
    /// Fallos que <b>abren el circuito</b>: errores HTTP transitorios (5xx, 408) y el timeout
    /// por intento. Son síntomas de un servicio enfermo.
    /// <para>
    /// 429 queda deliberadamente fuera: un rate limit significa que la API está <i>sana</i> y
    /// respondiendo correctamente, solo que vamos demasiado rápido. Abrir el circuito por eso
    /// bloquearía 30s todos los endpoints de la cuenta basándose en una política de cuotas que
    /// el servidor ya comunica con precisión vía <c>Retry-After</c>.
    /// </para>
    /// <para>
    /// ⚠️ <b>Efecto colateral a tener presente:</b> Polly trata todo resultado que no maneja
    /// como un éxito, así que un 429 <b>reinicia</b> la racha de fallos consecutivos. Una API
    /// que alterne 5xx y 429 no abrirá el circuito. Es el mismo punto ciego que tiene la racha
    /// ante cualquier éxito intercalado, documentado en <see cref="CircuitBreakerOptions"/>.
    /// </para>
    /// </summary>
    private static PolicyBuilder<HttpResponseMessage> HandledFailures() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>();

    /// <summary>
    /// Fallos que <b>se reintentan</b>: los del breaker más 429.
    /// <para>
    /// ⚠️ <b>A definir con el equipo</b> si 429 debe además abrir el circuito. Hoy no lo hace,
    /// por el motivo documentado en <see cref="HandledFailures"/>.
    /// </para>
    /// </summary>
    private static PolicyBuilder<HttpResponseMessage> RetriableFailures() =>
        HandledFailures()
            .OrResult(response => (int)response.StatusCode == TooManyRequests);

    private static IAsyncPolicy<HttpResponseMessage> BuildRetry(
        FacturamaOptions options,
        ILogger log,
        Func<double> randomSample,
        Func<DateTimeOffset> utcNow) =>
        RetriableFailures()
            .WaitAndRetryAsync(
                retryCount: options.Retry.MaxRetries,
                sleepDurationProvider: (attempt, outcome, _) =>
                    ResolveDelay(options.Retry, attempt, outcome, randomSample(), utcNow()),
                onRetryAsync: (outcome, timespan, attempt, _) =>
                {
                    log.LogWarning(
                        "Reintento {Attempt}/{MaxRetries} en {Seconds}s — {Reason}",
                        attempt,
                        options.Retry.MaxRetries,
                        timespan.TotalSeconds,
                        Describe(outcome));
                    return Task.CompletedTask;
                });

    /// <summary>
    /// Espera antes del siguiente intento. El <c>Retry-After</c> del servidor gana sobre el
    /// backoff calculado: es la única fuente que sabe cuándo estará libre la cuota. Si no lo
    /// envía, se usa el backoff exponencial con jitter.
    /// </summary>
    internal static TimeSpan ResolveDelay(
        RetryOptions retry,
        int attempt,
        DelegateResult<HttpResponseMessage> outcome,
        double jitter,
        DateTimeOffset utcNow) =>
        RetryAfter(outcome.Result, utcNow) is { } serverDelay
            ? Min(serverDelay, retry.MaxDelay)
            : JitteredBackoffDelay(retry, attempt, jitter);

    /// <summary>
    /// Lee la cabecera <c>Retry-After</c>, que la API puede enviar en segundos relativos o como
    /// fecha absoluta. Devuelve <c>null</c> si no viene o si ya venció.
    /// </summary>
    private static TimeSpan? RetryAfter(HttpResponseMessage? response, DateTimeOffset utcNow)
    {
        var retryAfter = response?.Headers.RetryAfter;

        if (retryAfter is null)
            return null;

        var delay = retryAfter.Delta ?? (retryAfter.Date is { } date ? date - utcNow : (TimeSpan?)null);

        return delay is null || delay <= TimeSpan.Zero ? null : delay;
    }

    private static string Describe(DelegateResult<HttpResponseMessage> outcome) =>
        outcome.Exception?.Message ?? $"HTTP {(int?)outcome.Result?.StatusCode}";

    /// <summary>
    /// Backoff exponencial <b>sin jitter</b>: <c>BaseDelay * 2^(intento-1)</c>, acotado por
    /// <see cref="RetryOptions.MaxDelay"/>. <c>BaseDelay</c> es el multiplicador del primer
    /// intento, no la base del exponente.
    /// <para>
    /// Es el <b>techo</b> de la espera: lo que realmente se duerme lo decide
    /// <see cref="JitteredBackoffDelay"/>. Se usa tal cual para calcular el presupuesto.
    /// </para>
    /// </summary>
    internal static TimeSpan BackoffDelay(RetryOptions retry, int attempt) =>
        Min(Multiply(retry.BaseDelay, Math.Pow(2, attempt - 1)), retry.MaxDelay);

    /// <summary>
    /// Backoff exponencial con <b>equal jitter</b>: <c>techo/2 + aleatorio(0, techo/2)</c>.
    /// <para>
    /// Sin jitter, N consumidores que fallan a la vez reintentan en el mismo milisegundo y
    /// vuelven a tumbar el servicio que intentan alcanzar. Se reparte la mitad superior de la
    /// espera y se conserva la mitad inferior como piso, para que un sorteo bajo no degenere
    /// en un reintento inmediato.
    /// </para>
    /// </summary>
    /// <param name="jitter">Muestra aleatoria en [0,1). Fuera de rango se satura.</param>
    internal static TimeSpan JitteredBackoffDelay(RetryOptions retry, int attempt, double jitter) =>
        Multiply(BackoffDelay(retry, attempt), 0.5 + 0.5 * Clamp01(jitter));

    /// <summary>
    /// Satura a [0,1]. Escrito con la comparación positiva a propósito: con <c>NaN</c> toda
    /// comparación es falsa, así que la forma negada dejaría pasar el <c>NaN</c> al cálculo.
    /// </summary>
    private static double Clamp01(double value) =>
        value > 0 ? (value < 1 ? value : 1) : 0;

    private static TimeSpan Min(TimeSpan left, TimeSpan right) =>
        left < right ? left : right;

    /// <summary>
    /// Multiplica una duración por un factor. El operador <c>*</c> de <see cref="TimeSpan"/> no
    /// existe en netstandard2.0; esto replica su redondeo a ticks enteros.
    /// </summary>
    private static TimeSpan Multiply(TimeSpan duration, double factor) =>
        TimeSpan.FromTicks((long)Math.Round(duration.Ticks * factor));

    /// <summary>
    /// Presupuesto total de la operación: todos los intentos más la <b>peor</b> espera posible
    /// entre cada uno.
    /// <para>
    /// La peor espera no es el backoff: un <c>Retry-After</c> del servidor lo sustituye y puede
    /// llegar hasta <see cref="RetryOptions.MaxDelay"/>. Si el presupuesto ignorara ese caso,
    /// el techo de <c>HttpClient.Timeout</c> cortaría la operación a mitad de una espera
    /// legítima y el consumidor recibiría un timeout en vez del rate limit real.
    /// </para>
    /// </summary>
    internal static TimeSpan CalculateTotalBudget(FacturamaOptions options)
    {
        if (!options.Retry.Enabled)
            return options.Timeout + SafetyMargin;

        // La peor espera de cada intento es siempre MaxDelay: acota por construcción tanto al
        // backoff como al Retry-After, así que no hay que recorrer los intentos uno por uno.
        var waits = Multiply(options.Retry.MaxDelay, options.Retry.MaxRetries);

        var attempts = options.Retry.MaxRetries + 1;
        return Multiply(options.Timeout, attempts) + waits + SafetyMargin;
    }
}
