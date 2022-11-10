using ownable.Models.Indexed;

namespace ownable.Handlers;

public interface IIndexableHandler
{
    Task<bool> HandleBatchAsync(List<Received> batch, CancellationToken cancellationToken);
}