using FacturamaNetSDK.Configuration;
using FacturamaNetSDK.Http;

namespace FacturamaNetSDK.Tests.Http;

public sealed class ResiliencePolicyTests
{
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
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromSeconds(2) };

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
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromMilliseconds(baseDelayMs) };

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
    // Presupuesto total: el techo global no puede cortar los reintentos
    // -------------------------------------------------------------------------

    /// <summary>
    /// Regresión: antes <c>HttpClient.Timeout</c> valía <c>options.Timeout</c> (30s) mientras
    /// el backoff por sí solo consumía 14s, así que el techo global mataba la operación
    /// a mitad de los reintentos.
    /// </summary>
    [Fact]
    public void CalculateTotalBudget_CubreTodosLosIntentosMasElBackoff()
    {
        var options = Options(new RetryOptions
        {
            Enabled = true,
            MaxRetries = 3,
            BaseDelay = TimeSpan.FromSeconds(2)
        });

        var budget = FacturamaHttpClientFactory.CalculateTotalBudget(options);

        // 4 intentos * 30s + (2s + 4s + 8s) de backoff + 5s de margen
        Assert.Equal(TimeSpan.FromSeconds(139), budget);
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
