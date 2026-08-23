namespace FacturamaNetSDK.Internal;

/// <summary>
/// Validaciones de argumentos portables entre targets.
/// <c>ArgumentNullException.ThrowIfNull</c> es net6+ y no existe en netstandard2.0.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Lanza <see cref="ArgumentNullException"/> cuando <paramref name="value"/> es nulo.
    /// </summary>
    internal static void NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
    }
}
