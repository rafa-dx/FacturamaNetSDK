#if !NET5_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// El compilador exige este tipo para emitir setters <c>init</c>, y netstandard2.0 no lo
    /// incluye. Definirlo aquí habilita los <c>record</c> y las propiedades <c>init</c> de los
    /// modelos sin duplicarlos por target.
    /// </summary>
    internal static class IsExternalInit
    {
    }
}

#endif
