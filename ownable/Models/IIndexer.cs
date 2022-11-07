using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace ownable.Models;

public interface IIndexer
{
    Task IndexAsync(IWeb3 web3, string rootAddress, BlockParameter fromBlock, BlockParameter toBlock, CancellationToken cancellationToken);
}