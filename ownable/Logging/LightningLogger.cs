using Microsoft.Extensions.Logging;

namespace ownable.Logging;

internal sealed class LightningLogger : ILogger
{
    private readonly LightningLogStore _store;

    public LightningLogger(LightningLogStore store) => _store = store;

    public IDisposable BeginScope<TState>(TState state) => new EmptyDisposable();
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;
        _store.Append(logLevel, eventId, state, exception, formatter);
    }

    private sealed class EmptyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}