using Microsoft.Extensions.Logging;

namespace FacturamaNetSDK.Tests.TestDoubles;

/// <summary>
/// Logger de prueba: acumula los mensajes ya formateados para poder afirmar sobre ellos.
/// </summary>
internal sealed class CapturingLogger : ILogger
{
    internal List<string> Messages { get; } = new();

    internal int CountContaining(string fragment) =>
        Messages.Count(m => m.Contains(fragment, StringComparison.OrdinalIgnoreCase));

    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        Messages.Add(formatter(state, exception));

    private sealed class NullScope : IDisposable
    {
        internal static readonly NullScope Instance = new();

        public void Dispose() { }
    }
}
