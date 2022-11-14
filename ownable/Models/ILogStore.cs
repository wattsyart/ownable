using ownable.Models;

namespace ownable.Logging;

public interface ILogStore
{
    IEnumerable<LogEntry> Get(CancellationToken cancellationToken = default);
    IEnumerable<LogEntry> GetByKey(string key, CancellationToken cancellationToken = default);
    IEnumerable<LogEntry> GetByKeyAndValue(string key, string? value, CancellationToken cancellationToken = default);
}