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
/// Comportamiento del circuit breaker construido desde <see cref="CircuitBreakerOptions"/>.
/// Ejercita la política directamente, sin red: el delegado cuenta cuántos intentos la
/// atraviesan realmente.
/// </summary>
public sealed class CircuitBreakerPolicyTests
{
    private static readonly RetryOptions NoRetry = new() { Enabled = false };

    private static readonly TimeSpan Instant = TimeSpan.FromMilliseconds(1);

    private static readonly TimeSpan LongBreak = TimeSpan.FromMinutes(1);

    private static CircuitBreakerOptions Breaker(int failures) =>
        new()
        {
            FailuresBeforeBreaking = failures,
            BreakDuration = LongBreak
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
        var policy = PolicyFor(NoRetry, Breaker(failures: 3));
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
        var policy = PolicyFor(NoRetry, Breaker(failures: 4));
        var call = new FailingCall();

        for (var i = 0; i < 3; i++)
            await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        var response = await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(4, call.Calls);
    }

    /// <summary>
    /// Un éxito intercalado reinicia el contador: el breaker cuenta fallos <b>consecutivos</b>.
    /// </summary>
    [Fact]
    public async Task Racha_UnExitoIntercalado_ReiniciaElContadorDeFallos()
    {
        var policy = PolicyFor(NoRetry, Breaker(failures: 3));
        var call = new FailingCall();

        var opened = await AlternateUntilBroken(policy, call, maxIterations: 30);

        Assert.False(opened, "Alternando fallo/éxito nunca hay 3 fallos seguidos.");
    }

    /// <summary>
    /// El punto ciego de la racha, explícito: una API con degradación <b>parcial</b> —que
    /// alterna fallos y éxitos— no abre el circuito por muchas operaciones que fallen.
    /// <para>
    /// Cubrirlo exigiría una política por proporción sobre ventana deslizante. Se evaluó y se
    /// dejó fuera de 1.0.0: requiere un volumen sostenido que un consumidor de facturación
    /// típico no alcanza, y añadir opciones después no rompe a nadie, quitarlas sí.
    /// </para>
    /// </summary>
    [Fact]
    public async Task Racha_LaDegradacionParcialNoAbreElCircuito()
    {
        var log = new CapturingLogger();
        var policy = SelectorFor(NoRetry, Breaker(failures: 3), log)(Request(HttpMethod.Get));
        var call = new FailingCall();

        var opened = await AlternateUntilBroken(policy, call, maxIterations: 40);

        Assert.False(opened);
        Assert.Equal(0, log.CountContaining("fallos consecutivos"));
    }

    // -------------------------------------------------------------------------
    // Interacción con los reintentos
    // -------------------------------------------------------------------------

    /// <summary>
    /// El breaker envuelve al retry, así que una operación que agota sus intentos registra
    /// <b>un solo</b> fallo. Con umbral 2 hacen falta dos operaciones completas para abrir,
    /// aunque entre ambas hayan pasado 6 intentos por la red.
    /// </summary>
    [Fact]
    public async Task UnaOperacionCompleta_CuentaComoUnSoloFalloDelBreaker()
    {
        var retry = new RetryOptions { MaxRetries = 2, BaseDelay = Instant };
        var policy = PolicyFor(retry, Breaker(failures: 2));
        var call = new FailingCall();

        await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(3, call.Calls);

        await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(6, call.Calls);

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => policy.ExecuteAsync(call.Invoke, CancellationToken.None));

        Assert.Equal(6, call.Calls);
    }

    /// <summary>
    /// Regresión: cuando el breaker estaba dentro del retry, un umbral igual o menor a los
    /// intentos de una operación dejaba el circuito abierto con una sola petición fallida, y
    /// hacía falta una validación cruzada con <c>RetryOptions</c> para impedirlo. Con el
    /// breaker por fuera la combinación es inofensiva: sigue contando operaciones.
    /// </summary>
    [Fact]
    public async Task ConUmbralMenorALosIntentos_UnaSolaOperacionNoAbreElCircuito()
    {
        var retry = new RetryOptions { MaxRetries = 5, BaseDelay = Instant };
        var policy = PolicyFor(retry, Breaker(failures: 2));
        var call = new FailingCall();

        var response = await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(6, call.Calls);

        var second = await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, second.StatusCode);
    }

    /// <summary>
    /// Con el circuito abierto la petición falla antes de entrar al retry: no consume intentos
    /// ni esperas de backoff.
    /// </summary>
    [Fact]
    public async Task ConElCircuitoAbierto_NoSeConsumenReintentos()
    {
        var retry = new RetryOptions { MaxRetries = 3, BaseDelay = Instant };
        var policy = PolicyFor(retry, Breaker(failures: 2));
        var call = new FailingCall();

        await policy.ExecuteAsync(call.Invoke, CancellationToken.None);
        await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        var callsBeforeOpen = call.Calls;

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => policy.ExecuteAsync(call.Invoke, CancellationToken.None));

        Assert.Equal(callsBeforeOpen, call.Calls);
    }

    /// <summary>
    /// Los defaults de fábrica (3 reintentos, racha 5) toleran cuatro operaciones completas
    /// fallidas antes de abrir el circuito.
    /// </summary>
    [Fact]
    public async Task ConLosDefaults_CuatroOperacionesFallidasNoAbrenElCircuito()
    {
        var retry = new RetryOptions { BaseDelay = Instant };
        var policy = PolicyFor(retry, new CircuitBreakerOptions { BreakDuration = LongBreak });
        var call = new FailingCall();

        for (var i = 0; i < 4; i++)
        {
            var response = await policy.ExecuteAsync(call.Invoke, CancellationToken.None);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        Assert.Equal(16, call.Calls);
    }

    /// <summary>
    /// La quinta operación fallida alcanza el default de <c>FailuresBeforeBreaking</c> y abre
    /// el circuito para toda la cuenta.
    /// </summary>
    [Fact]
    public async Task ConLosDefaults_LaQuintaOperacionFallidaAbreElCircuito()
    {
        var retry = new RetryOptions { BaseDelay = Instant };
        var policy = PolicyFor(retry, new CircuitBreakerOptions { BreakDuration = LongBreak });
        var call = new FailingCall();

        for (var i = 0; i < 5; i++)
            await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => policy.ExecuteAsync(call.Invoke, CancellationToken.None));

        Assert.Equal(20, call.Calls);
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
            Breaker(failures: 2));
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
        var breaker = Breaker(failures: 2) with
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

    // -------------------------------------------------------------------------
    // Rate limit (429): se reintenta, pero no es síntoma de un servicio enfermo
    // -------------------------------------------------------------------------

    private sealed class RateLimitedCall
    {
        internal int Calls { get; private set; }

        internal Task<HttpResponseMessage> Invoke(CancellationToken _)
        {
            Calls++;
            return Task.FromResult(new HttpResponseMessage((HttpStatusCode)429));
        }
    }

    /// <summary>
    /// Regresión: <c>HandleTransientHttpError()</c> de Polly cubre 5xx y 408, pero <b>no</b> 429.
    /// Sin añadirlo explícitamente, el error más común de una API de facturación bajo carga
    /// era el único que la política de reintentos ignoraba.
    /// </summary>
    [Fact]
    public async Task RateLimit_SeReintenta()
    {
        var retry = new RetryOptions { MaxRetries = 2, BaseDelay = Instant, MaxDelay = Instant };
        var policy = PolicyFor(retry, Breaker(failures: 100));
        var call = new RateLimitedCall();

        await policy.ExecuteAsync(call.Invoke, CancellationToken.None);

        Assert.Equal(3, call.Calls);
    }

    /// <summary>
    /// Un 429 significa que la API está sana y respondiendo bien, solo que vamos rápido.
    /// Abrir el circuito bloquearía 30s toda la cuenta por una política de cuotas que el
    /// servidor ya comunica con precisión vía Retry-After.
    /// </summary>
    [Fact]
    public async Task RateLimit_NoAbreElCircuito()
    {
        var policy = PolicyFor(NoRetry, Breaker(failures: 2));
        var call = new RateLimitedCall();

        for (var i = 0; i < 10; i++)
        {
            var response = await policy.ExecuteAsync(call.Invoke, CancellationToken.None);
            Assert.Equal(429, (int)response.StatusCode);
        }

        Assert.Equal(10, call.Calls);
    }

    /// <summary>
    /// Consecuencia de dejar 429 fuera de los fallos del breaker: Polly trata todo resultado no
    /// manejado como un <b>éxito</b>, así que un 429 intercalado <b>reinicia</b> la racha de 5xx.
    /// <para>
    /// Es coherente con la semántica elegida —el servidor respondió, está sano—, pero implica
    /// que una API que alterna 5xx y 429 no abre el circuito por racha. De ese patrón se
    /// encarga la capa de ratio, y solo si el consumidor la habilitó.
    /// </para>
    /// </summary>
    [Fact]
    public async Task RateLimit_ReiniciaLaRachaDeFallos()
    {
        var policy = PolicyFor(NoRetry, Breaker(failures: 2));
        var rateLimited = new RateLimitedCall();
        var failing = new FailingCall();

        await policy.ExecuteAsync(failing.Invoke, CancellationToken.None);      // racha = 1
        await policy.ExecuteAsync(rateLimited.Invoke, CancellationToken.None);  // 429 → racha = 0

        // Sin el 429 en medio, esta operación habría sido la segunda seguida y habría abierto.
        var stillClosed = await policy.ExecuteAsync(failing.Invoke, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, stillClosed.StatusCode);

        await policy.ExecuteAsync(failing.Invoke, CancellationToken.None);      // racha = 2 → abre

        await Assert.ThrowsAnyAsync<BrokenCircuitException>(
            () => policy.ExecuteAsync(failing.Invoke, CancellationToken.None));
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

