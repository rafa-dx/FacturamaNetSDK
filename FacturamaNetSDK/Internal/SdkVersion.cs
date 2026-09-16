using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FacturamaNetSDK.Internal;

/// <summary>
/// Versión del SDK y User-Agent que se envía en cada petición HTTP.
/// </summary>
internal static class SdkVersion
{
    private const string ProductName = "FacturamaDotNetSDK";

    /// <summary>
    /// Último recurso si el assembly no expone metadatos de versión (escenarios de
    /// hosting exóticos o ensamblados generados en memoria).
    /// </summary>
    private const string FallbackVersion = "1.0.0";

    /// <summary>
    /// Versión del paquete NuGet, con sufijo de prerelease si lo hay ("1.2.3-beta.1").
    /// Se lee de los metadatos del assembly para no duplicar el número del .csproj.
    /// </summary>
    internal static string Version { get; } = ResolvePackageVersion();

    /// <summary>
    /// User-Agent de las peticiones.
    /// Ejemplo: "Facturama-DotNet-SDK/1.0.0 (.NET 8.0.30; Microsoft Windows 10.0.26200; X64)".
    /// </summary>
    internal static string UserAgent { get; } = BuildUserAgent(
        RuntimeInformation.FrameworkDescription,
        RuntimeInformation.OSDescription,
        RuntimeInformation.ProcessArchitecture);

    /// <summary>
    /// Construye el User-Agent a partir de los datos del entorno. Recibe el entorno por
    /// parámetro para poder fijarlo en pruebas.
    /// </summary>
    internal static string BuildUserAgent(string framework, string os, Architecture architecture) =>
        $"{ProductName}/{Version} ({framework}; {os}; {architecture})";

    private static string ResolvePackageVersion()
    {
        var assembly = typeof(SdkVersion).Assembly;
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informational))
            return assembly.GetName().Version?.ToString() ?? FallbackVersion;

        return StripBuildMetadata(informational!);
    }

    /// <summary>
    /// Descarta el sufijo "+sha" que SourceLink añade a la versión informacional:
    /// el '+' no es válido en el token de versión de un User-Agent.
    /// </summary>
    private static string StripBuildMetadata(string version)
    {
        var separator = version.IndexOf("+", StringComparison.Ordinal);
        return separator < 0 ? version : version.Substring(0, separator);
    }
}
