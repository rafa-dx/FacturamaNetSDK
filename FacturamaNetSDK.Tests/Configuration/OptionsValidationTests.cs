using FacturamaNetSDK.Configuration;

namespace FacturamaNetSDK.Tests.Configuration;

public sealed class CircuitBreakerOptionsTests
{
    [Fact]
    public void PorDefecto_EstaHabilitadoConLosUmbralesDocumentados()
    {
        var breaker = new CircuitBreakerOptions();

        Assert.True(breaker.Enabled);
        Assert.Equal(TimeSpan.FromSeconds(30), breaker.BreakDuration);
        Assert.Equal(10, breaker.FailuresBeforeBreaking);
        Assert.Equal(0.5, breaker.FailureRatio);
        Assert.Equal(TimeSpan.FromSeconds(60), breaker.SamplingDuration);
        Assert.Equal(20, breaker.MinimumThroughput);
    }

    [Fact]
    public void PorDefecto_NoLanza()
    {
        new CircuitBreakerOptions().Validate();
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void FailuresBeforeBreaking_MenorA2_Lanza(int failures)
    {
        var breaker = new CircuitBreakerOptions { FailuresBeforeBreaking = failures };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => breaker.Validate());
        Assert.Equal(nameof(CircuitBreakerOptions.FailuresBeforeBreaking), ex.ParamName);
    }

    [Fact]
    public void FailuresBeforeBreaking_Igual2_EsElMinimoAceptado()
    {
        new CircuitBreakerOptions { FailuresBeforeBreaking = 2 }.Validate();
    }

    [Fact]
    public void BreakDuration_Cero_Lanza()
    {
        var breaker = new CircuitBreakerOptions { BreakDuration = TimeSpan.Zero };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => breaker.Validate());
        Assert.Equal(nameof(CircuitBreakerOptions.BreakDuration), ex.ParamName);
    }

    [Fact]
    public void BreakDuration_Negativa_Lanza()
    {
        var breaker = new CircuitBreakerOptions { BreakDuration = TimeSpan.FromSeconds(-1) };

        Assert.Throws<ArgumentOutOfRangeException>(() => breaker.Validate());
    }

    [Fact]
    public void BreakDuration_UnTick_EsValida()
    {
        new CircuitBreakerOptions { BreakDuration = TimeSpan.FromTicks(1) }.Validate();
    }

    // --- Capa de ratio ---

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    public void FailureRatio_FueraDeRango_Lanza(double ratio)
    {
        var breaker = new CircuitBreakerOptions { FailureRatio = ratio };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => breaker.Validate());
        Assert.Equal(nameof(CircuitBreakerOptions.FailureRatio), ex.ParamName);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(0.5)]
    [InlineData(1)]
    public void FailureRatio_EnRango_EsValido(double ratio)
    {
        new CircuitBreakerOptions { FailureRatio = ratio }.Validate();
    }

    /// <summary>
    /// Polly rechaza ventanas por debajo de la resolución de su temporizador; validarlo aquí
    /// convierte el fallo en un error de configuración claro en vez de una excepción de Polly.
    /// </summary>
    [Fact]
    public void SamplingDuration_BajoLaResolucionDelTemporizador_Lanza()
    {
        var breaker = new CircuitBreakerOptions
        {
            SamplingDuration = CircuitBreakerOptions.MinimumSamplingDuration - TimeSpan.FromTicks(1)
        };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => breaker.Validate());
        Assert.Equal(nameof(CircuitBreakerOptions.SamplingDuration), ex.ParamName);
    }

    [Fact]
    public void SamplingDuration_EnLaResolucionExacta_EsValida()
    {
        new CircuitBreakerOptions
        {
            SamplingDuration = CircuitBreakerOptions.MinimumSamplingDuration
        }.Validate();
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(1)]
    public void MinimumThroughput_MenorA2_Lanza(int throughput)
    {
        var breaker = new CircuitBreakerOptions { MinimumThroughput = throughput };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => breaker.Validate());
        Assert.Equal(nameof(CircuitBreakerOptions.MinimumThroughput), ex.ParamName);
    }

    [Fact]
    public void MinimumThroughput_Igual2_EsElMinimoAceptado()
    {
        new CircuitBreakerOptions { MinimumThroughput = 2 }.Validate();
    }

    /// <summary>
    /// Apagado el breaker, sus umbrales son irrelevantes: validarlos obligaría a rellenar
    /// valores coherentes para una política que no se construye.
    /// </summary>
    [Fact]
    public void Deshabilitado_NoValidaSusUmbrales()
    {
        new CircuitBreakerOptions
        {
            Enabled = false,
            FailuresBeforeBreaking = 0,
            BreakDuration = TimeSpan.Zero,
            FailureRatio = 0,
            SamplingDuration = TimeSpan.Zero,
            MinimumThroughput = 0
        }.Validate();
    }
}

public sealed class RetryOptionsValidationTests
{
    [Fact]
    public void PorDefecto_NoLanza()
    {
        new RetryOptions().Validate();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void MaxRetries_Negativo_Lanza(int maxRetries)
    {
        var retry = new RetryOptions { MaxRetries = maxRetries };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => retry.Validate());
        Assert.Equal(nameof(RetryOptions.MaxRetries), ex.ParamName);
    }

    /// <summary>
    /// Regresión: sin tope, <c>BaseDelay * 2^(intento-1)</c> desborda el <c>TimeSpan</c>
    /// del presupuesto total y revienta con <c>OverflowException</c> dentro del SDK.
    /// </summary>
    [Theory]
    [InlineData(11)]
    [InlineData(64)]
    [InlineData(int.MaxValue)]
    public void MaxRetries_SobreElTope_Lanza(int maxRetries)
    {
        var retry = new RetryOptions { MaxRetries = maxRetries };

        Assert.Throws<ArgumentOutOfRangeException>(() => retry.Validate());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(RetryOptions.MaxRetriesLimit)]
    public void MaxRetries_EnLosLimites_EsValido(int maxRetries)
    {
        new RetryOptions { MaxRetries = maxRetries }.Validate();
    }

    [Fact]
    public void BaseDelay_Cero_Lanza()
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.Zero };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => retry.Validate());
        Assert.Equal(nameof(RetryOptions.BaseDelay), ex.ParamName);
    }

    [Fact]
    public void BaseDelay_Negativo_Lanza()
    {
        var retry = new RetryOptions { BaseDelay = TimeSpan.FromMilliseconds(-1) };

        Assert.Throws<ArgumentOutOfRangeException>(() => retry.Validate());
    }

    [Fact]
    public void Deshabilitado_NoValidaSusUmbrales()
    {
        new RetryOptions
        {
            Enabled = false,
            MaxRetries = -5,
            BaseDelay = TimeSpan.Zero
        }.Validate();
    }

    [Fact]
    public void MaxAttemptsPerOperation_PorDefecto_EsElInicialMasLosReintentos()
    {
        Assert.Equal(4, new RetryOptions { MaxRetries = 3 }.MaxAttemptsPerOperation);
    }

    [Fact]
    public void MaxAttemptsPerOperation_ConReintentosApagados_EsUno()
    {
        Assert.Equal(1, new RetryOptions { Enabled = false, MaxRetries = 3 }.MaxAttemptsPerOperation);
    }

    /// <summary>
    /// Con todos los verbos apagados no hay reintento posible, aunque
    /// <see cref="RetryOptions.Enabled"/> siga en true.
    /// </summary>
    [Fact]
    public void MaxAttemptsPerOperation_SinNingunVerboHabilitado_EsUno()
    {
        var retry = new RetryOptions
        {
            MaxRetries = 3,
            RetryGet = false,
            RetryPost = false,
            RetryPut = false,
            RetryDelete = false
        };

        Assert.Equal(1, retry.MaxAttemptsPerOperation);
    }
}

public sealed class FacturamaOptionsValidationTests
{
    private static FacturamaOptions Valid() =>
        new() { Username = "usuario", Password = "secreto" };

    [Fact]
    public void PorDefecto_LosDefaultsDeFabricaSonCoherentes()
    {
        Valid().Validate();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SinUsername_Lanza(string? username)
    {
        var options = Valid();
        options.Username = username!;

        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal(nameof(FacturamaOptions.Username), ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SinPassword_Lanza(string? password)
    {
        var options = Valid();
        options.Password = password!;

        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal(nameof(FacturamaOptions.Password), ex.ParamName);
    }

    [Fact]
    public void TimeoutCero_Lanza()
    {
        var options = Valid();
        options.Timeout = TimeSpan.Zero;

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
        Assert.Equal(nameof(FacturamaOptions.Timeout), ex.ParamName);
    }

    /// <summary>
    /// Regresión: esta era la configuración del proyecto Sandbox. El breaker cuenta intentos,
    /// así que con 3 intentos por operación y umbral 3 una única petición fallida abría el
    /// circuito para toda la cuenta y devolvía 503 en lugar del error real.
    /// </summary>
    [Fact]
    public void UmbralIgualALosIntentosDeUnaOperacion_Lanza()
    {
        var options = Valid();
        options.Retry = new RetryOptions { MaxRetries = 2 };
        options.CircuitBreaker = new CircuitBreakerOptions { FailuresBeforeBreaking = 3 };

        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal(nameof(FacturamaOptions.CircuitBreaker), ex.ParamName);
    }

    [Fact]
    public void UmbralMenorALosIntentosDeUnaOperacion_Lanza()
    {
        var options = Valid();
        options.Retry = new RetryOptions { MaxRetries = 5 };
        options.CircuitBreaker = new CircuitBreakerOptions { FailuresBeforeBreaking = 3 };

        Assert.Throws<ArgumentException>(() => options.Validate());
    }

    /// <summary>
    /// La capa de ratio también cuenta intentos: con <c>MinimumThroughput</c> por debajo de los
    /// intentos de una operación, sus 4 fallos dan un ratio del 100% y abren el circuito solos.
    /// </summary>
    [Fact]
    public void MinimumThroughputIgualALosIntentosDeUnaOperacion_Lanza()
    {
        var options = Valid();
        options.Retry = new RetryOptions { MaxRetries = 3 };
        options.CircuitBreaker = new CircuitBreakerOptions { MinimumThroughput = 4 };

        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal(nameof(FacturamaOptions.CircuitBreaker), ex.ParamName);
        Assert.Contains(nameof(CircuitBreakerOptions.MinimumThroughput), ex.Message);
    }

    [Fact]
    public void MinimumThroughputUnoPorEncimaDeLosIntentos_EsElMinimoAceptado()
    {
        var options = Valid();
        options.Retry = new RetryOptions { MaxRetries = 3 };
        options.CircuitBreaker = new CircuitBreakerOptions { MinimumThroughput = 5 };

        options.Validate();
    }

    [Fact]
    public void UmbralUnoPorEncimaDeLosIntentos_EsElMinimoAceptado()
    {
        var options = Valid();
        options.Retry = new RetryOptions { MaxRetries = 2 };
        options.CircuitBreaker = new CircuitBreakerOptions { FailuresBeforeBreaking = 4 };

        options.Validate();
    }

    [Fact]
    public void SinReintentos_ElUmbralMinimoSigueSiendoDos()
    {
        var options = Valid();
        options.Retry = new RetryOptions { Enabled = false };
        options.CircuitBreaker = new CircuitBreakerOptions { FailuresBeforeBreaking = 2 };

        options.Validate();
    }

    [Fact]
    public void BreakerDeshabilitado_NoExigeRelacionConLosReintentos()
    {
        var options = Valid();
        options.Retry = new RetryOptions { MaxRetries = 10 };
        options.CircuitBreaker = new CircuitBreakerOptions { Enabled = false, FailuresBeforeBreaking = 2 };

        options.Validate();
    }

    /// <summary>
    /// Con todos los verbos apagados una operación gasta un solo intento, así que
    /// la validación cruzada no debe exigir un umbral alto.
    /// </summary>
    [Fact]
    public void ReintentosSinVerbosHabilitados_NoExigeUmbralAlto()
    {
        var options = Valid();
        options.Retry = new RetryOptions
        {
            MaxRetries = 8,
            RetryGet = false,
            RetryPost = false,
            RetryPut = false,
            RetryDelete = false
        };
        options.CircuitBreaker = new CircuitBreakerOptions { FailuresBeforeBreaking = 2 };

        options.Validate();
    }

    [Fact]
    public void LasOpcionesAnidadasInvalidas_SePropagan()
    {
        var options = Valid();
        options.Retry = new RetryOptions { BaseDelay = TimeSpan.Zero };

        Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
    }
}
