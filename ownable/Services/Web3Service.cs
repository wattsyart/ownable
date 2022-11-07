using Microsoft.Extensions.Options;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using ownable.Indexers;
using ownable.Models;

namespace ownable.Services;

internal sealed class Web3Service
{
    private readonly IOptionsMonitor<Web3Options> _options;
    private readonly IEnumerable<IIndexer> _indexers;
    
    public Web3Service(IOptionsMonitor<Web3Options> options, IEnumerable<IIndexer> indexers)
    {
        _options = options;
        _indexers = indexers;
    }

    public async Task IndexAddressAsync(string address, CancellationToken cancellationToken)
    {
        var uri = new Uri(_options.CurrentValue.RpcUrl!);
        var client = new RpcClient(uri);
        var web3 = new Web3(client);

        foreach (var indexer in _indexers)
            await indexer.IndexAsync(web3, address, cancellationToken);
    }
}