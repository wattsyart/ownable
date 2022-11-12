using System.Numerics;
using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using ownable.Contracts;
using ownable.Data;
using ownable.Models;
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

    protected async Task IndexTransfersAsync<TEvent>(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken) where TEvent : ITokenEvent, ITransferEvent, new()
    {
        await IndexReceivedAsync<TEvent>(web3, account, fromBlock, toBlock, scope, cancellationToken);
        await IndexSentAsync<TEvent>(web3, account, fromBlock, toBlock, scope, cancellationToken);
    }

    protected async Task IndexReceivedAsync<TEvent>(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken) 
        where TEvent : ITransferEvent, ITokenEvent, new()
    {
        var receivedTokens = await _tokenService.GetReceivedTokensAsync<TEvent>(web3, account, fromBlock: fromBlock, toBlock: toBlock, cancellationToken, _logger);

        foreach (var received in receivedTokens)
            await IndexTokenContractAsync(web3, received.ContractAddress, received.TokenId, received.BlockNumber, scope, cancellationToken);
    }

    protected async Task IndexSentAsync<TEvent>(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken) 
        where TEvent : ITransferEvent, ITokenEvent, new()
    {
        var sentTokens = await _tokenService.GetSentTokensAsync<TEvent>(web3, account, fromBlock: fromBlock, toBlock: toBlock, cancellationToken, _logger);

        foreach (var sent in sentTokens)
            await IndexTokenContractAsync(web3, sent.ContractAddress, sent.TokenId, sent.BlockNumber, scope, cancellationToken);
    }

    protected abstract Task IndexTokenContractAsync(IWeb3 web3, string contractAddress, BigInteger tokenId, BigInteger blockNumber, IndexScope scope, CancellationToken cancellationToken);

    protected async Task<ContractFeatures> GetContractFeaturesAsync(IWeb3 web3, string contractAddress, ERCSpecification specification)
    {
        var features = ContractFeatures.None;

        if (specification.standardInterface != null && await SupportsInterface(web3, contractAddress, specification.standardInterface))
            features |= ContractFeatures.SupportsStandard;

        if (specification.metadataInterface != null && await SupportsInterface(web3, contractAddress, specification.metadataInterface))
            features |= ContractFeatures.SupportsMetadata;

        if (specification.uriInterface != null && await SupportsInterface(web3, contractAddress, specification.uriInterface))
            features |= ContractFeatures.SupportsUri;

        if (specification.nameInterface != null && await SupportsInterface(web3, contractAddress, specification.nameInterface))
            features |= ContractFeatures.SupportsName;

        if (specification.symbolInterface != null && await SupportsInterface(web3, contractAddress, specification.symbolInterface))
            features |= ContractFeatures.SupportsSymbol;

        return features;
    }
    
    private static async Task<bool> SupportsInterface(IWeb3 web3, string contractAddress, byte[] interfaceId)
    {
        var supportsInterfaceQuery = web3.Eth.GetContractQueryHandler<ERC165.SupportsInterfaceFunction>();
        var supportsInterface = await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction { InterfaceId = interfaceId });
        return supportsInterface;
    }
}