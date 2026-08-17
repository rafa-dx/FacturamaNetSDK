using FacturamaNetSDK.Configuration;
using FacturamaNetSDK.Http;
using FacturamaNetSDK.Tests.TestDoubles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.CircuitBreaker;
using System.Net;

namespace FacturamaNetSDK.Tests.Http;

/// <summary>
/// Comportamiento de las dos capas del circuit breaker construidas desde
/// <see cref="CircuitBreakerOptions"/>. Ejercita la política directamente, sin red: el delegado
/// cuenta cuántos intentos la atraviesan realmente.
/// </summary>
public sealed class CircuitBreakerPolicyTests
{
    private static readonly RetryOptions NoRetry = new() { Enabled = false };

    private static readonly TimeSpan Instant = TimeSpan.FromMilliseconds(1);

    private static readonly TimeSpan LongBreak = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Aísla la capa de racha: un <c>MinimumThroughput</c> inalcanzable deja fuera de juego
    /// a la de ratio.
    /// </summary>
    private static CircuitBreakerOptions ConsecutiveOnly(int failures) =>
        new()
        {
            FailuresBeforeBreaking = failures,
            BreakDuration = LongBreak,
            MinimumThroughput = 1_000
        };

    /// <summary>
    /// Aísla la capa de ratio: un umbral de racha inalcanzable deja fuera de juego a la otra.
    /// </summary>
    private static CircuitBreakerOptions RatioOnly(
        double ratio,
        int minimumThroughput,
        TimeSpan? sampling = null) =>
        new()
        {
            FailuresBeforeBreaking = 1_000,
            BreakDuration = LongBreak,
            FailureRatio = ratio,
            MinimumThroughput = minimumThroughput,
            SamplingDuration = sampling ?? TimeSpan.FromMinutes(5)
        };

    private static Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> SelectorFor(
        RetryOptions retry,
        CircuitBreakerOptions breaker,
        ILogger? log = null) =>
        FacturamaHttpClientFactory.BuildPolicySelector(
            new FacturamaOptions
            {
                Username = "usuario",
                Password = "secreto",
                Retry = retry,
                CircuitBreaker = breaker
            },
            log ?? NullLogger.Instance);

    private static IAsyncPolicy<HttpResponseMessage> PolicyFor(
        RetryOptions retry,
        CircuitBreakerOptions breaker,
        HttpMethod? method = null) =>
        SelectorFor(retry, breaker)(Request(method ?? HttpMethod.Get));

    private static HttpRequestMessage Request(HttpMethod method) =>
        new(method, "https://apisandbox.facturama.mx/Product");

    private static Task<HttpResponseMessage> Succeed(CancellationToken _) =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

    private sealed class FailingCall
    {
        internal int Calls { get; private set; }

        internal Task<HttpResponseMessage> Invoke(CancellationToken _)
        {
            Calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }
    }

    /// <summary>Ejecuta el patrón fallo/éxito alternado hasta que el circuito abra.</summary>
    private static async Task<bool> AlternateUntilBroken(
        IAsyncPolicy<HttpResponseMessage> policy,
        FailingCall call,
        int maxIterations = 40)
    {
        for (var i = 0; i < maxIterations; i++)
        {
            try
            {
                await policy.ExecuteAsync(
                    i % 2 == 0 ? call.Invoke : Succeed,
                    CancellationToken.None);
            }
            catch (BrokenCircuitException)
            {
                return true;
            }
        }

        return false;
    }

    // -------------------------------------------------------------------------
    // Capa 1 — racha de fallos consecutivos
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Racha_AlAlcanzarElUmbral_AbreYDejaDeTocarLaRed()
    {
        var policy = PolicyFor(NoRetry, ConsecutiveOnly(failures: 3));
        var call = new FailingCall();

        for (var i = 0; i < 3; i++)
            await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => policy.ExecuteAsync(call.Invoke, CancellationToken.None));

        Assert.Equal(3, call.Calls);
    }

    [Fact]
    public async Task Racha_PorDebajoDelUmbral_MantieneElCircuitoCerrado()
    {
        var policy = PolicyFor(NoRetry, ConsecutiveOnly(failures: 4));
        var call = new FailingCall();

        for (var i = 0; i < 3; i++)
            await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        var response = await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(4, call.Calls);
    }

    /// <summary>
    /// Un éxito intercalado reinicia el contador: esta capa cuenta fallos <b>consecutivos</b>.
    /// Es su punto ciego, y la razón de existir de la capa de ratio.
    /// </summary>
    [Fact]
    public async Task Racha_UnExitoIntercalado_ReiniciaElContadorDeFallos()
    {
        var policy = PolicyFor(NoRetry, ConsecutiveOnly(failures: 3));
        var call = new FailingCall();

        var opened = await AlternateUntilBroken(policy, call, maxIterations: 30);

        Assert.False(opened, "Alternando fallo/éxito nunca hay 3 fallos seguidos.");
    }

    // -------------------------------------------------------------------------
    // Capa 2 — proporción de fallos en ventana deslizante
    // -------------------------------------------------------------------------

    /// <summary>
    /// Contrapunto exacto de <see cref="Racha_UnExitoIntercalado_ReiniciaElContadorDeFallos"/>:
    /// con el mismo tráfico donde la racha jamás dispara, el ratio sí detecta la degradación.
    /// </summary>
    [Fact]
    public async Task Ratio_ConExitosIntercalados_AbrePorProporcion()
    {
        var policy = PolicyFor(NoRetry, RatioOnly(ratio: 0.5, minimumThroughput: 10));
        var call = new FailingCall();

        var opened = await AlternateUntilBroken(policy, call);

        Assert.True(opened, "Con 50% de fallos sostenido el ratio debe abrir el circuito.");
    }

    /// <summary>
    /// Punto ciego de esta capa: por debajo de <c>MinimumThroughput</c> no actúa nunca, por
    /// muchos fallos consecutivos que haya. De ese escenario se encarga la capa de racha.
    /// </summary>
    [Fact]
    public async Task Ratio_PorDebajoDelThroughputMinimo_NoAbreNunca()
    {
        var policy = PolicyFor(NoRetry, RatioOnly(ratio: 0.5, minimumThroughput: 10));
        var call = new FailingCall();

        for (var i = 0; i < 9; i++)
            await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(9, call.Calls);
    }

    [Fact]
    public async Task Ratio_AlAlcanzarElThroughputConTodoFallos_Abre()
    {
        var policy = PolicyFor(NoRetry, RatioOnly(ratio: 0.5, minimumThroughput: 5));
        var call = new FailingCall();

        for (var i = 0; i < 5; i++)
            await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => policy.ExecuteAsync(call.Invoke, CancellationToken.None));

        Assert.Equal(5, call.Calls);
    }

    /// <summary>La ventana es deslizante: los fallos anteriores a ella se olvidan.</summary>
    [Fact]
    public async Task Ratio_OlvidaLosFallosFueraDeLaVentana()
    {
        var policy = PolicyFor(
            NoRetry,
            RatioOnly(ratio: 0.5, minimumThroughput: 4, sampling: TimeSpan.FromMilliseconds(300)));
        var call = new FailingCall();

        for (var i = 0; i < 3; i++)
            await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        await Task.Delay(TimeSpan.FromMilliseconds(600));

        var response = await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(4, call.Calls);
    }

    // -------------------------------------------------------------------------
    // Las dos capas encadenadas
    // -------------------------------------------------------------------------

    /// <summary>
    /// Consumidor de bajo volumen: nunca alcanza el <c>MinimumThroughput</c> del ratio, así que
    /// la protección tiene que venir de la racha.
    /// </summary>
    [Fact]
    public async Task Encadenadas_LaRachaCubreElVolumenBajo()
    {
        var breaker = new CircuitBreakerOptions
        {
            FailuresBeforeBreaking = 3,
            MinimumThroughput = 50,
            BreakDuration = LongBreak
        };
        var policy = PolicyFor(NoRetry, breaker);
        var call = new FailingCall();

        for (var i = 0; i < 3; i++)
            await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => policy.ExecuteAsync(call.Invoke, CancellationToken.None));

        Assert.Equal(3, call.Calls);
    }

    /// <summary>
    /// Degradación parcial bajo carga: nunca hay 3 fallos seguidos, así que la racha no dispara
    /// y la protección tiene que venir del ratio.
    /// </summary>
    [Fact]
    public async Task Encadenadas_ElRatioCubreLaDegradacionParcial()
    {
        var breaker = new CircuitBreakerOptions
        {
            FailuresBeforeBreaking = 3,
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromMinutes(5),
            BreakDuration = LongBreak
        };
        var policy = PolicyFor(NoRetry, breaker);
        var call = new FailingCall();

        var opened = await AlternateUntilBroken(policy, call);

        Assert.True(opened);
    }

    /// <summary>
    /// Sin cascada entre capas: cuando la de ratio (interna) abre, su
    /// <c>BrokenCircuitException</c> no está entre los fallos que maneja la de racha (externa),
    /// así que esta no la cuenta ni abre a su vez. El log reporta una sola capa, la culpable.
    /// </summary>
    [Fact]
    public async Task Encadenadas_LaAperturaDeUnaCapaNoArrastraALaOtra()
    {
        var log = new CapturingLogger();
        var breaker = new CircuitBreakerOptions
        {
            FailuresBeforeBreaking = 3,
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromMinutes(5),
            BreakDuration = LongBreak
        };
        var policy = SelectorFor(NoRetry, breaker, log)(Request(HttpMethod.Get));
        var call = new FailingCall();

        await AlternateUntilBroken(policy, call);

        for (var i = 0; i < 10; i++)
            await Assert.ThrowsAnyAsync<BrokenCircuitException>(
                () => policy.ExecuteAsync(call.Invoke, CancellationToken.None));

        Assert.Equal(1, log.CountContaining("de las peticiones falló"));
        Assert.Equal(0, log.CountContaining("fallos consecutivos"));
    }

    // -------------------------------------------------------------------------
    // Interacción con los reintentos
    // -------------------------------------------------------------------------

    /// <summary>
    /// El breaker está dentro del retry, así que cada reintento incrementa su contador.
    /// Con 3 intentos por operación y umbral 4, la primera operación deja el contador en 3
    /// y la segunda abre el circuito en su primer intento.
    /// </summary>
    [Fact]
    public async Task CadaReintento_CuentaComoUnFalloDelBreaker()
    {
        var retry = new RetryOptions { MaxRetries = 2, BaseDelay = Instant };
        var policy = PolicyFor(retry, ConsecutiveOnly(failures: 4));
        var call = new FailingCall();

        await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(3, call.Calls);

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => policy.ExecuteAsync(call.Invoke, CancellationToken.None));

        Assert.Equal(4, call.Calls);
    }

    /// <summary>
    /// Regresión: con el umbral igual a los intentos de una operación, una única petición
    /// fallida agota el contador y deja el circuito abierto. La operación en curso devuelve
    /// su 500 con normalidad, pero la <b>siguiente</b> — aunque sea a otro endpoint — falla
    /// sin tocar la red. <c>FacturamaOptions.Validate</c> ahora rechaza esta combinación.
    /// </summary>
    [Fact]
    public async Task ConUmbralIgualALosIntentos_UnaSolaOperacionDejaElCircuitoAbierto()
    {
        var retry = new RetryOptions { MaxRetries = 2, BaseDelay = Instant };
        var policy = PolicyFor(retry, ConsecutiveOnly(failures: 3));
        var call = new FailingCall();

        var response = await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(3, call.Calls);

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => policy.ExecuteAsync(call.Invoke, CancellationToken.None));

        Assert.Equal(3, call.Calls);
    }

    /// <summary>
    /// Los defaults de fábrica (3 reintentos, racha 10, throughput 20) toleran una operación
    /// completa fallida sin abrir el circuito por ninguna de las dos capas.
    /// </summary>
    [Fact]
    public async Task ConLosDefaults_UnaOperacionFallidaNoAbreElCircuito()
    {
        var retry = new RetryOptions { BaseDelay = Instant };
        var policy = PolicyFor(retry, new CircuitBreakerOptions());
        var call = new FailingCall();

        var response = await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(4, call.Calls);
    }

    // -------------------------------------------------------------------------
    // Estado compartido, recuperación y apagado
    // -------------------------------------------------------------------------

    /// <summary>
    /// La fábrica construye el breaker una sola vez: los fallos de un POST (no reintentado)
    /// abren el mismo circuito que luego bloquea a un GET.
    /// </summary>
    [Fact]
    public async Task ElBreaker_EsCompartidoPorTodosLosVerbos()
    {
        var selector = SelectorFor(
            new RetryOptions { BaseDelay = Instant },
            ConsecutiveOnly(failures: 2));
        var postPolicy = selector(Request(HttpMethod.Post));
        var getPolicy = selector(Request(HttpMethod.Get));
        var call = new FailingCall();

        await postPolicy.ExecuteAsync(call.Invoke, CancellationToken.None);
        await postPolicy.ExecuteAsync(call.Invoke, CancellationToken.None);

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => getPolicy.ExecuteAsync(call.Invoke, CancellationToken.None));

        Assert.Equal(2, call.Calls);
    }

    [Fact]
    public async Task TrasBreakDuration_PasaAHalfOpenYSeCierraConUnExito()
    {
        var breaker = ConsecutiveOnly(failures: 2) with
        {
            BreakDuration = TimeSpan.FromMilliseconds(150)
        };
        var policy = PolicyFor(NoRetry, breaker);
        var call = new FailingCall();

        await policy.ExecuteAsync(call.Invoke, CancellationToken.None);
        await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => policy.ExecuteAsync(call.Invoke, CancellationToken.None));

        await Task.Delay(TimeSpan.FromMilliseconds(400));

        var response = await policy.ExecuteAsync(Succeed, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Deshabilitado_NingunaCapaAbreElCircuito()
    {
        var policy = PolicyFor(NoRetry, new CircuitBreakerOptions { Enabled = false });
        var call = new FailingCall();

        for (var i = 0; i < 40; i++)
            await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(40, call.Calls);
    }
}
