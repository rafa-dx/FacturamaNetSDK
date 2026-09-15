using FacturamaNetSDK.Configuration;
using FacturamaNetSDK.Http;
using Polly;
using System.Net;
using System.Net.Http.Headers;

namespace FacturamaNetSDK.Tests.Http;

public sealed class ResiliencePolicyTests
{
    /// <summary>Tope inalcanzable: deja fuera de juego a <c>MaxDelay</c>.</summary>
    private static readonly TimeSpan NoCap = TimeSpan.FromMinutes(5);

    private static FacturamaOptions Options(RetryOptions retry, TimeSpan? timeout = null) =>
        new()
        {
            Username = "usuario",
            Password = "secreto",
            Timeout = timeout ?? TimeSpan.FromSeconds(30),
            Retry = retry
        };

    // -------------------------------------------------------------------------
    // Backoff: BaseDelay es multiplicador, no base del exponente
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(3, 8)]
    [InlineData(4, 16)]
    public void BackoffDelay_ConBaseDe2Segundos_DuplicaCadaIntento(int attempt, int expectedSeconds)
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromSeconds(2), MaxDelay = NoCap };

        var delay = FacturamaHttpClientFactory.BackoffDelay(retry, attempt);

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), delay);
    }

    /// <summary>
    /// Regresión: con la fórmula anterior (<c>BaseDelay^intento</c>) un BaseDelay menor
    /// a 1s producía un backoff <b>decreciente</b>, martillando un servicio caído.
    /// </summary>
    [Theory]
    [InlineData(500)]
    [InlineData(750)]
    [InlineData(1000)]
    [InlineData(2000)]
    public void BackoffDelay_ConCualquierBase_EsSiempreCreciente(int baseDelayMs)
    {
        var retry = new RetryOptions
        {
            BaseDelay = TimeSpan.FromMilliseconds(baseDelayMs),
            MaxDelay = NoCap
        };

        var delays = Enumerable.Range(1, 5)
            .Select(attempt => FacturamaHttpClientFactory.BackoffDelay(retry, attempt))
            .ToList();

        for (var i = 1; i < delays.Count; i++)
            Assert.True(delays[i] > delays[i - 1],
                $"El intento {i + 1} ({delays[i]}) debe esperar más que el {i} ({delays[i - 1]}).");
    }

    [Fact]
    public void BackoffDelay_ElPrimerIntento_EsperaExactamenteBaseDelay()
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromMilliseconds(500) };

        Assert.Equal(TimeSpan.FromMilliseconds(500), FacturamaHttpClientFactory.BackoffDelay(retry, 1));
    }

    // -------------------------------------------------------------------------
    // Tope de la espera: sin él, el crecimiento exponencial se desboca
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sin tope, con BaseDelay de 2s el décimo reintento esperaría más de 17 minutos y
    /// desbordaría el presupuesto de la operación.
    /// </summary>
    [Theory]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(10)]
    public void BackoffDelay_NuncaSuperaMaxDelay(int attempt)
    {
        var retry = new RetryOptions
        {
            BaseDelay = TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(10)
        };

        Assert.Equal(TimeSpan.FromSeconds(10), FacturamaHttpClientFactory.BackoffDelay(retry, attempt));
    }

    [Fact]
    public void BackoffDelay_ConLosDefaults_ElTopeNoSeActiva()
    {
        var retry = new RetryOptions();

        // 2s, 4s, 8s — todas por debajo del MaxDelay de 10s.
        Assert.Equal(TimeSpan.FromSeconds(2), FacturamaHttpClientFactory.BackoffDelay(retry, 1));
        Assert.Equal(TimeSpan.FromSeconds(4), FacturamaHttpClientFactory.BackoffDelay(retry, 2));
        Assert.Equal(TimeSpan.FromSeconds(8), FacturamaHttpClientFactory.BackoffDelay(retry, 3));
    }

    // -------------------------------------------------------------------------
    // Jitter: reparte la mitad superior de la espera
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sin jitter, N consumidores que fallan a la vez reintentan en el mismo instante y
    /// vuelven a tumbar el servicio. El piso de la mitad inferior evita que un sorteo bajo
    /// degenere en un reintento inmediato.
    /// </summary>
    [Theory]
    [InlineData(0.0, 1000)]
    [InlineData(0.25, 1250)]
    [InlineData(0.5, 1500)]
    [InlineData(1.0, 2000)]
    public void JitteredBackoffDelay_RepartelaMitadSuperiorDelTecho(double jitter, int expectedMs)
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromSeconds(2), MaxDelay = NoCap };

        var delay = FacturamaHttpClientFactory.JitteredBackoffDelay(retry, attempt: 1, jitter);

        Assert.Equal(TimeSpan.FromMilliseconds(expectedMs), delay);
    }

    [Theory]
    [InlineData(-5.0, 1000)]
    [InlineData(5.0, 2000)]
    [InlineData(double.NaN, 1000)]
    public void JitteredBackoffDelay_ConJitterFueraDeRango_Satura(double jitter, int expectedMs)
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromSeconds(2), MaxDelay = NoCap };

        var delay = FacturamaHttpClientFactory.JitteredBackoffDelay(retry, attempt: 1, jitter);

        Assert.Equal(TimeSpan.FromMilliseconds(expectedMs), delay);
    }

    [Fact]
    public void JitteredBackoffDelay_NuncaSuperaElTechoSinJitter()
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromSeconds(2), MaxDelay = NoCap };

        foreach (var attempt in Enumerable.Range(1, 5))
        {
            var ceiling = FacturamaHttpClientFactory.BackoffDelay(retry, attempt);

            foreach (var jitter in new[] { 0.0, 0.3, 0.7, 1.0 })
            {
                var delay = FacturamaHttpClientFactory.JitteredBackoffDelay(retry, attempt, jitter);

                Assert.InRange(delay, TimeSpan.FromTicks(ceiling.Ticks / 2), ceiling);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Retry-After: el servidor manda sobre el backoff calculado
    // -------------------------------------------------------------------------

    private static readonly DateTimeOffset Now = new(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);

    private static DelegateResult<HttpResponseMessage> RateLimited(RetryConditionHeaderValue? retryAfter)
    {
        var response = new HttpResponseMessage((HttpStatusCode)429);

        if (retryAfter is not null)
            response.Headers.RetryAfter = retryAfter;

        return new DelegateResult<HttpResponseMessage>(response);
    }

    [Fact]
    public void ResolveDelay_ConRetryAfterEnSegundos_LoHonra()
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromSeconds(2), MaxDelay = NoCap };
        var outcome = RateLimited(new RetryConditionHeaderValue(TimeSpan.FromSeconds(7)));

        var delay = FacturamaHttpClientFactory.ResolveDelay(retry, 1, outcome, jitter: 1.0, Now);

        Assert.Equal(TimeSpan.FromSeconds(7), delay);
    }

    [Fact]
    public void ResolveDelay_ConRetryAfterComoFecha_LoConvierteADuracion()
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromSeconds(2), MaxDelay = NoCap };
        var outcome = RateLimited(new RetryConditionHeaderValue(Now.AddSeconds(12)));

        var delay = FacturamaHttpClientFactory.ResolveDelay(retry, 1, outcome, jitter: 1.0, Now);

        Assert.Equal(TimeSpan.FromSeconds(12), delay);
    }

    /// <summary>
    /// Un <c>Retry-After</c> desmedido no puede colgar al consumidor más allá del presupuesto
    /// que el SDK ya reservó en <c>HttpClient.Timeout</c>.
    /// </summary>
    [Fact]
    public void ResolveDelay_ConRetryAfterMayorAMaxDelay_LoAcota()
    {
        var retry = new RetryOptions { MaxDelay = TimeSpan.FromSeconds(10) };
        var outcome = RateLimited(new RetryConditionHeaderValue(TimeSpan.FromMinutes(30)));

        var delay = FacturamaHttpClientFactory.ResolveDelay(retry, 1, outcome, jitter: 1.0, Now);

        Assert.Equal(TimeSpan.FromSeconds(10), delay);
    }

    [Fact]
    public void ResolveDelay_SinRetryAfter_CaeAlBackoffConJitter()
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromSeconds(2), MaxDelay = NoCap };
        var outcome = RateLimited(retryAfter: null);

        var delay = FacturamaHttpClientFactory.ResolveDelay(retry, 2, outcome, jitter: 0.0, Now);

        // Techo del intento 2 = 4s; con jitter 0 se duerme la mitad.
        Assert.Equal(TimeSpan.FromSeconds(2), delay);
    }

    /// <summary>Una fecha ya vencida no debe producir una espera negativa.</summary>
    [Fact]
    public void ResolveDelay_ConRetryAfterVencido_CaeAlBackoff()
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromSeconds(2), MaxDelay = NoCap };
        var outcome = RateLimited(new RetryConditionHeaderValue(Now.AddSeconds(-30)));

        var delay = FacturamaHttpClientFactory.ResolveDelay(retry, 1, outcome, jitter: 1.0, Now);

        Assert.Equal(TimeSpan.FromSeconds(2), delay);
    }

    [Fact]
    public void ResolveDelay_SinRespuesta_CaeAlBackoff()
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromSeconds(2), MaxDelay = NoCap };
        var outcome = new DelegateResult<HttpResponseMessage>(new HttpRequestException("sin red"));

        var delay = FacturamaHttpClientFactory.ResolveDelay(retry, 1, outcome, jitter: 1.0, Now);

        Assert.Equal(TimeSpan.FromSeconds(2), delay);
    }

    // -------------------------------------------------------------------------
    // Presupuesto total: el techo global no puede cortar los reintentos
    // -------------------------------------------------------------------------

    /// <summary>
    /// Regresión: antes <c>HttpClient.Timeout</c> valía <c>options.Timeout</c> (30s) mientras
    /// el backoff por sí solo consumía 14s, así que el techo global mataba la operación
    /// a mitad de los reintentos.
    /// </summary>
    [Fact]
    public void CalculateTotalBudget_CubreTodosLosIntentosMasLaPeorEspera()
    {
        var options = Options(new RetryOptions
        {
            Enabled = true,
            MaxRetries = 3,
            BaseDelay = TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(10)
        });

        var budget = FacturamaHttpClientFactory.CalculateTotalBudget(options);

        // 4 intentos * 30s + 3 esperas * 10s + 5s de margen.
        // La peor espera es MaxDelay, no el backoff (2s/4s/8s): un Retry-After del servidor
        // sustituye al backoff y puede llegar hasta el tope.
        Assert.Equal(TimeSpan.FromSeconds(155), budget);
    }

    /// <summary>
    /// Regresión: si el presupuesto solo cubriera el backoff calculado, un <c>Retry-After</c>
    /// legítimo cerca del tope haría que <c>HttpClient.Timeout</c> matara la operación a mitad
    /// de la espera y el consumidor recibiría un timeout en vez del rate limit real.
    /// </summary>
    [Fact]
    public void CalculateTotalBudget_CubreElPeorRetryAfterPosible()
    {
        var retry = new RetryOptions
        {
            Enabled = true,
            MaxRetries = 3,
            BaseDelay = TimeSpan.FromSeconds(1),
            MaxDelay = TimeSpan.FromSeconds(20)
        };
        var options = Options(retry, TimeSpan.FromSeconds(10));

        var budget = FacturamaHttpClientFactory.CalculateTotalBudget(options);
        var worstWaits = TimeSpan.FromTicks(retry.MaxDelay.Ticks * retry.MaxRetries);

        Assert.True(budget > worstWaits + options.Timeout,
            $"El presupuesto ({budget}) debe cubrir 3 esperas al tope ({worstWaits}) más un intento.");
    }

    [Fact]
    public void CalculateTotalBudget_ConLosDefaults_SonSetentaYCincoSegundos()
    {
        var options = new FacturamaOptions { Username = "usuario", Password = "secreto" };

        // 4 intentos * 10s + 3 esperas * 10s + 5s de margen
        Assert.Equal(TimeSpan.FromSeconds(75), FacturamaHttpClientFactory.CalculateTotalBudget(options));
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(3, 2)]
    [InlineData(5, 1)]
    public void CalculateTotalBudget_SiempreExcedeElBackoffAcumulado(int maxRetries, int baseDelaySeconds)
    {
        var retry = new RetryOptions
        {
            Enabled = true,
            MaxRetries = maxRetries,
            BaseDelay = TimeSpan.FromSeconds(baseDelaySeconds)
        };
        var options = Options(retry);

        var budget = FacturamaHttpClientFactory.CalculateTotalBudget(options);
        var totalBackoff = Enumerable.Range(1, maxRetries)
            .Aggregate(TimeSpan.Zero, (sum, a) => sum + FacturamaHttpClientFactory.BackoffDelay(retry, a));

        Assert.True(budget > totalBackoff + options.Timeout,
            $"El presupuesto ({budget}) debe superar el backoff acumulado ({totalBackoff}) más un intento completo.");
    }

    [Fact]
    public void CalculateTotalBudget_SinReintentos_EsUnSoloIntentoMasMargen()
    {
        var options = Options(new RetryOptions { Enabled = false }, TimeSpan.FromSeconds(20));

        var budget = FacturamaHttpClientFactory.CalculateTotalBudget(options);

        Assert.Equal(TimeSpan.FromSeconds(25), budget);
    }

    [Fact]
    public void CalculateTotalBudget_ConMasReintentos_CreceMonotonicamente()
    {
        var budgets = new[] { 1, 2, 3, 4 }
            .Select(retries => FacturamaHttpClientFactory.CalculateTotalBudget(
                Options(new RetryOptions { Enabled = true, MaxRetries = retries })))
            .ToList();

        for (var i = 1; i < budgets.Count; i++)
            Assert.True(budgets[i] > budgets[i - 1]);
    }

    // -------------------------------------------------------------------------
    // Selección de reintentos por verbo
    // -------------------------------------------------------------------------

    [Fact]
    public void ShouldRetry_PorDefecto_ReintentaIdempotentesYNoPost()
    {
        var retry = new RetryOptions();

        Assert.True(retry.ShouldRetry(HttpMethod.Get));
        Assert.True(retry.ShouldRetry(HttpMethod.Put));
        Assert.True(retry.ShouldRetry(HttpMethod.Delete));
        Assert.False(retry.ShouldRetry(HttpMethod.Post));
    }

    [Fact]
    public void ShouldRetry_ConPostHabilitado_LoReintenta()
    {
        var retry = new RetryOptions { RetryPost = true };

        Assert.True(retry.ShouldRetry(HttpMethod.Post));
    }

    [Fact]
    public void ShouldRetry_VerboNoContemplado_NoReintenta()
    {
        var retry = new RetryOptions();

        Assert.False(retry.ShouldRetry(HttpMethod.Head));
        Assert.False(retry.ShouldRetry(HttpMethod.Options));
    }

    // -------------------------------------------------------------------------
    // Construcción de clientes
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_ConOpcionesNulas_Lanza()
    {
        Assert.Throws<ArgumentNullException>(() => new FacturamaHttpClientFactory(null!));
    }

    [Fact]
    public void CreateRootClient_YCreateApiClient_DevuelvenInstanciasIndependientes()
    {
        var factory = new FacturamaHttpClientFactory(Options(new RetryOptions()));

        using var root = factory.CreateRootClient();
        using var api = factory.CreateApiClient("3");

        Assert.NotSame(root, api);
    }
}
