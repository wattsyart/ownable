using ownable.Logging;

namespace ownable.Models;

public interface ILogStore : IIndex
{
    IEnumerable<LogEntry> Get(CancellationToken cancellationToken = default);
    IEnumerable<LogEntry> GetByKey(string key, CancellationToken cancellationToken = default);
    IEnumerable<LogEntry> GetByKeyAndValue(string key, string? value, CancellationToken cancellationToken = default);
}