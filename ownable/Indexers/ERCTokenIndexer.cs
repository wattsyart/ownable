using System.Numerics;
using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using ownable.Contracts;
using ownable.Data;
using ownable.Models;
using ownable.Models.Indexed;
using ownable.Services;

namespace ownable.Indexers;

public abstract class ERCTokenIndexer : IBlockIndexer
{
    private readonly Store _store;
    private readonly TokenService _tokenService;
    private readonly ILogger<IBlockIndexer> _logger;

    protected ERCTokenIndexer(Store store, TokenService tokenService, ILogger<IBlockIndexer> logger)
    {
        _store = store;
        _tokenService = tokenService;
        _logger = logger;
    }

    public abstract Task IndexAccountAsync(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken);

    public async Task IndexCollectionAsync<TEvent>(IWeb3 web3, string contractAddress, BlockParameter fromBlock, BlockParameter toBlock,
        IndexScope scope, CancellationToken cancellationToken) where TEvent : ITokenEvent, ITransferEvent, new()
    {
        string? continuationToken = null;
        var page = await _tokenService.GetMintedTokensAsync<TEvent>(web3, contractAddress, fromBlock, toBlock, cancellationToken, continuationToken, _logger);
        foreach (var token in page.Value ?? Enumerable.Empty<Received>())
            await IndexTokenAsync(web3, contractAddress, token.TokenId, token.BlockNumber, scope, cancellationToken);
    }

    protected async Task IndexTransfersAsync<TEvent>(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken) where TEvent : ITokenEvent, ITransferEvent, new()
    {
        await IndexReceivedAsync<TEvent>(web3, account, fromBlock, toBlock, scope, cancellationToken);
        await IndexSentAsync<TEvent>(web3, account, fromBlock, toBlock, scope, cancellationToken);
    }

    protected async Task IndexReceivedAsync<TEvent>(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken) 
        where TEvent : ITransferEvent, ITokenEvent, new()
    {
        var receivedTokens = await _tokenService.GetReceivedTokensAsync<TEvent>(web3, account, fromBlock: fromBlock, toBlock: toBlock, cancellationToken, _logger);

        if (scope.HasFlagFast(IndexScope.TokenTransfers))
        {
            foreach (var sent in receivedTokens)
                _store.Save(sent, cancellationToken);
        }
    }

    protected async Task IndexSentAsync<TEvent>(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken) 
        where TEvent : ITransferEvent, ITokenEvent, new()
    {
        var sentTokens = await _tokenService.GetSentTokensAsync<TEvent>(web3, account, fromBlock: fromBlock, toBlock: toBlock, cancellationToken, _logger);

        if (scope.HasFlagFast(IndexScope.TokenTransfers))
        {
            foreach (var sent in sentTokens)
                _store.Save(sent, cancellationToken);
        }
    }

    protected abstract Task IndexTokenAsync(IWeb3 web3, string contractAddress, BigInteger tokenId, BigInteger blockNumber, IndexScope scope, CancellationToken cancellationToken);
}