using FacturamaNetSDK.Internal;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FacturamaNetSDK.Tests.Internal;

public sealed class SdkVersionTests
{
    // -------------------------------------------------------------------------
    // Resolución de la versión
    // -------------------------------------------------------------------------

    [Fact]
    public void Version_CoincideConLaVersionInformacionalDelAssembly()
    {
        var informational = typeof(SdkVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        var expected = informational.Split('+')[0];

        Assert.Equal(expected, SdkVersion.Version);
    }

    [Fact]
    public void Version_NoArrastraElSufijoDeMetadatosDeCompilacion()
    {
        Assert.DoesNotContain("+", SdkVersion.Version, StringComparison.Ordinal);
    }

    [Fact]
    public void Version_NoEstaVacia()
    {
        Assert.False(string.IsNullOrWhiteSpace(SdkVersion.Version));
    }

    // -------------------------------------------------------------------------
    // Formato del User-Agent
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildUserAgent_UsaElFormatoDocumentado()
    {
        var userAgent = SdkVersion.BuildUserAgent(
            ".NET 8.0.30", "Microsoft Windows 10.0.26200", Architecture.X64);

        Assert.Equal(
            $"FacturamaDotNetSDK/{SdkVersion.Version} (.NET 8.0.30; Microsoft Windows 10.0.26200; X64)",
            userAgent);
    }

    [Fact]
    public void UserAgent_EmpiezaConElProductoYLaVersion()
    {
        Assert.StartsWith($"FacturamaDotNetSDK/{SdkVersion.Version} (", SdkVersion.UserAgent,
            StringComparison.Ordinal);
    }

    [Fact]
    public void UserAgent_EsElMismoValorEnCadaLectura()
    {
        Assert.Same(SdkVersion.UserAgent, SdkVersion.UserAgent);
    }

    // -------------------------------------------------------------------------
    // Compatibilidad con el parser de cabeceras: FacturamaHttpClientFactory llama
    // a ParseAdd, que lanza FormatException si el User-Agent no es válido.
    // -------------------------------------------------------------------------

    [Fact]
    public void UserAgent_DelEntornoActual_EsAceptadoPorParseAdd()
    {
        AssertParseable(SdkVersion.UserAgent);
    }

    [Theory]
    // Windows
    [InlineData(".NET 8.0.30", "Microsoft Windows 10.0.26200")]
    [InlineData(".NET Framework 4.8.9032.0", "Microsoft Windows 10.0.19045")]
    // Debian: la versión del kernel trae paréntesis anidados
    [InlineData(".NET 8.0.30", "Linux 6.1.0-21-amd64 #1 SMP PREEMPT_DYNAMIC Debian 6.1.90-1 (2024-05-03)")]
    // Ubuntu
    [InlineData(".NET 8.0.30", "Linux 5.15.0-89-generic #99-Ubuntu SMP Fri Nov 3 12:00:00 UTC 2023")]
    // macOS: ':', ';', '~' y '/' en la descripción
    [InlineData(".NET 8.0.30", "Darwin 22.1.0 Darwin Kernel Version 22.1.0: Sun Oct  9 20:14:54 PDT 2022; root:xnu-8792.41.9~2/RELEASE_X86_64")]
    // Alpine: backslash
    [InlineData(".NET 8.0.30", @"Alpine Linux v3.18 \ build")]
    public void UserAgent_DeCualquierPlataforma_EsAceptadoPorParseAdd(string framework, string os)
    {
        AssertParseable(SdkVersion.BuildUserAgent(framework, os, Architecture.X64));
    }

    [Theory]
    [InlineData(Architecture.X86)]
    [InlineData(Architecture.X64)]
    [InlineData(Architecture.Arm)]
    [InlineData(Architecture.Arm64)]
    public void UserAgent_DeCualquierArquitectura_EsAceptadoPorParseAdd(Architecture architecture)
    {
        AssertParseable(SdkVersion.BuildUserAgent(".NET 8.0.30", "Linux 6.1.0", architecture));
    }

    private static void AssertParseable(string userAgent)
    {
        using var request = new HttpRequestMessage();

        request.Headers.UserAgent.ParseAdd(userAgent);

        Assert.Equal(userAgent, request.Headers.UserAgent.ToString());
    }
}
