using ownable.Models.Indexed;

namespace ownable.Models;

public interface IIndexableHandler
{
    Task<bool> HandleBatchAsync(List<Received> batch, CancellationToken cancellationToken);
    Task<bool> HandleBatchAsync(List<Sent> batch, CancellationToken cancellationToken);
}