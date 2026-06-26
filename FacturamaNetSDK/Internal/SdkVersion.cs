using System.Runtime.InteropServices;

namespace FacturamaNetSDK.Internal;

internal static class SdkVersion
{
    internal const string Version = "1.0.0";

    internal static readonly string UserAgent =
        $"Facturama-DotNet-SDK/{Version} ({RuntimeInformation.FrameworkDescription})";
}