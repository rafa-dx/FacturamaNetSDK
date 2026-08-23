namespace FacturamaNetSDK.Internal;

/// <summary>
/// Lectura del cuerpo de la respuesta portable entre targets.
/// <para>
/// Las sobrecargas de <see cref="System.Net.Http.HttpContent"/> que aceptan
/// <see cref="CancellationToken"/> son net5+. En netstandard2.0 el token se comprueba antes de
/// empezar a leer, pero <b>no interrumpe una lectura ya en curso</b>: ahí el corte lo pone el
/// <c>Timeout</c> del <see cref="System.Net.Http.HttpClient"/>.
/// </para>
/// </summary>
internal static class HttpContentExtensions
{
    /// <summary>Lee el cuerpo como texto.</summary>
    internal static Task<string> ReadStringAsync(
        this HttpContent content,
        CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsStringAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsStringAsync();
#endif
    }

    /// <summary>Lee el cuerpo como arreglo de bytes.</summary>
    internal static Task<byte[]> ReadByteArrayAsync(
        this HttpContent content,
        CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsByteArrayAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsByteArrayAsync();
#endif
    }
}
