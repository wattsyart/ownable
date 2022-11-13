using Microsoft.Extensions.Options;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using ownable.Models;

namespace ownable.Services;

internal sealed class IndexService
{
    private readonly IOptionsMonitor<Web3Options> _options;
    private readonly IEnumerable<IBlockIndexer> _indexers;
    
    public IndexService(IOptionsMonitor<Web3Options> options, IEnumerable<IBlockIndexer> indexers)
    {
        _options = options;
        _indexers = indexers;
    }

    public async Task IndexAccountAsync(string address, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken)
    {
        var uri = new Uri(_options.CurrentValue.RpcUrl!);
        var client = new RpcClient(uri);
        var web3 = new Web3(client);

        foreach (var indexer in _indexers)
            await indexer.IndexAccountAsync(web3, address, fromBlock, toBlock, scope, cancellationToken);
    }
}