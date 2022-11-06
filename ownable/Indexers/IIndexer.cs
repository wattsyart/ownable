using Nethereum.Web3;

namespace ownable.Indexers;

public interface IIndexer
{
    Task IndexAddressAsync(Web3 web3, string address);
}