using System.Numerics;
using Microsoft.Extensions.Logging;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
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
    private readonly EventService _eventService;
    private readonly ILogger<IBlockIndexer> _logger;

    protected ERCTokenIndexer(Store store, EventService eventService, ILogger<IBlockIndexer> logger)
    {
        _store = store;
        _eventService = eventService;
        _logger = logger;
    }

    public abstract Task IndexAsync(IWeb3 web3, string rootAddress, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken);

    protected async Task IndexTransfersAsync<TEvent>(IWeb3 web3, string rootAddress, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken) where TEvent : ITokenEvent, ITransferEvent, new()
    {
        await IndexReceivedAsync<TEvent>(web3, rootAddress, fromBlock, toBlock, scope, cancellationToken);
        await IndexSentAsync<TEvent>(web3, rootAddress, fromBlock, toBlock, scope, cancellationToken);
    }

    protected async Task IndexReceivedAsync<TEvent>(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken) 
        where TEvent : ITransferEvent, ITokenEvent, new()
    {
        var receivedTokens = await _eventService.GetReceivedTokensAsync<TEvent>(web3, account, fromBlock: fromBlock, toBlock: toBlock, cancellationToken, _logger);

        foreach (var received in receivedTokens)
        {
            if(scope.HasFlagFast(IndexScope.TokenContracts))
                await IndexContractAddress(web3, received.ContractAddress, received.TokenId, received.BlockNumber, scope, cancellationToken);
        }
    }

    protected async Task IndexSentAsync<T>(IWeb3 web3, string rootAddress, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken) 
        where T : ITransferEvent, ITokenEvent, new()
    {
        var @event = web3.Eth.GetEvent<T>();
        var sentByAddress = @event.CreateFilterInput(filterTopic1: new object[] { rootAddress }, filterTopic2: null);
        sentByAddress.FromBlock = fromBlock;
        sentByAddress.ToBlock = toBlock;

        var sentChangeLog = await @event.GetAllChangesAsync(sentByAddress);

        foreach (var change in sentChangeLog)
        {
            var contractAddress = change.Log.Address;
            var tokenId = change.Event.GetTokenId();
            var blockNumber = change.Log.BlockNumber;

            var sent = new Sent
            {
                BlockNumber = blockNumber,
                ContractAddress = contractAddress,
                Address = change.Event.From,
                TokenId = new HexBigInteger(tokenId)
            };

            _store.Append(sent, cancellationToken);

            await IndexContractAddress(web3, contractAddress, tokenId, blockNumber, scope, cancellationToken);
        }
    }

    protected abstract Task IndexContractAddress(IWeb3 web3, string contractAddress, BigInteger tokenId, BigInteger blockNumber, IndexScope scope, CancellationToken cancellationToken);

    protected async Task<ContractFeatures> GetContractFeaturesAsync(IWeb3 web3, string contractAddress,
        byte[]? standardInterface,
        byte[]? metadataInterface,
        byte[]? uriInterface,
        byte[]? nameInterface,
        byte[]? symbolInterface)
    {
        var features = ContractFeatures.None;

        if (standardInterface != null && await SupportsInterface(web3, contractAddress, standardInterface))
            features |= ContractFeatures.SupportsStandard;

        if (metadataInterface != null && await SupportsInterface(web3, contractAddress, metadataInterface))
            features |= ContractFeatures.SupportsMetadata;

        if (uriInterface != null && await SupportsInterface(web3, contractAddress, uriInterface))
            features |= ContractFeatures.SupportsUri;

        if (nameInterface != null && await SupportsInterface(web3, contractAddress, nameInterface))
            features |= ContractFeatures.SupportsName;

        if (symbolInterface != null && await SupportsInterface(web3, contractAddress, symbolInterface))
            features |= ContractFeatures.SupportsSymbol;

        return features;
    }

    protected async Task<string?> TryGetNameAsync<TNameFunction>(IWeb3 web3, string contractAddress, ContractFeatures features, BlockParameter atBlock) 
        where TNameFunction : FunctionMessage, IParameterlessStringFunction, new()
    {
        if (features.SupportsName())
        {
            try
            {
                var nameQuery = web3.Eth.GetContractQueryHandler<TNameFunction>();
                var name = await nameQuery.QueryAsync<string>(contractAddress, new TNameFunction());
                return name;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Contract Address {ContractAddress} at block {BlockNumber} failed to fetch token name", contractAddress, atBlock);
            }
        }

        return null;
    }

    protected async Task<string?> TryGetSymbolAsync<TSymbolFunction>(IWeb3 web3, string contractAddress, ContractFeatures features, BlockParameter atBlock)
        where TSymbolFunction : FunctionMessage, IParameterlessStringFunction, new()
    {
        if (features.SupportsSymbol())
        {
            try
            {
                var symbolQuery = web3.Eth.GetContractQueryHandler<TSymbolFunction>();
                var symbol = await symbolQuery.QueryAsync<string>(contractAddress, new TSymbolFunction());
                return symbol;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Contract Address {ContractAddress} at block {BlockNumber} failed to fetch token symbol", contractAddress, atBlock);
            }
        }

        return null;
    }

    protected async Task<string?> TryGetTokenUriAsync<TTokenUriFunction>(IWeb3 web3, string contractAddress, BigInteger tokenId, ContractFeatures features, BlockParameter atBlock)
        where TTokenUriFunction : FunctionMessage, ITokenUriFunction, new()
    {
        if (features.SupportsUri())
        {
            try
            {
                var tokenUriQuery = web3.Eth.GetContractQueryHandler<TTokenUriFunction>();
                var tokenUri = await tokenUriQuery.QueryAsync<string>(contractAddress, new TTokenUriFunction { TokenId = tokenId });
                return tokenUri;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Contract Address {ContractAddress} at block {BlockNumber} failed to fetch token URI", contractAddress, atBlock);
            }
        }

        return null;
    }

    private static async Task<bool> SupportsInterface(IWeb3 web3, string contractAddress, byte[] interfaceId)
    {
        var supportsInterfaceQuery = web3.Eth.GetContractQueryHandler<ERC165.SupportsInterfaceFunction>();
        var supportsInterface = await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction { InterfaceId = interfaceId });
        return supportsInterface;
    }
}