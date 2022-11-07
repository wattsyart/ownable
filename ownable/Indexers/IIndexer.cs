using Nethereum.Web3;

namespace ownable.Indexers;

public interface IIndexer
{
    Task IndexAddressAsync(IWeb3 web3, string address, CancellationToken cancellationToken);
}