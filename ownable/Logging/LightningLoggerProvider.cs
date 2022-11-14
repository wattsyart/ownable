using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ownable.Logging;

internal sealed class LightningLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, LightningLogger> _loggers = new();
    private readonly LightningLogStore _store;

    public LightningLoggerProvider(LightningLogStore store) => _store = store;
    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new LightningLogger(_store));
    public void Dispose() => _loggers.Clear();
}