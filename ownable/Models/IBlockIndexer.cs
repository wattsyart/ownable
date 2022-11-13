using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace ownable.Models;

public interface IBlockIndexer
{
    Task IndexAccountAsync(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken);
    Task IndexCollectionAsync(IWeb3 web3, string contractAddress, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken);
}