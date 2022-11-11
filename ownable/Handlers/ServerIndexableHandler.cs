using ownable.Data;
using ownable.Models;
using ownable.Models.Indexed;

namespace ownable.Handlers
{
    public sealed class ServerIndexableHandler : IIndexableHandler
    {
        private readonly Store _store;

        public ServerIndexableHandler(Store store)
        {
            _store = store;
        }

        public Task<bool> HandleBatchAsync(List<Received> batch, CancellationToken cancellationToken)
        {
            foreach (var item in batch)
            {
                _store.Save(item, cancellationToken);
            }

            return Task.FromResult(true);
        }
    }
}
