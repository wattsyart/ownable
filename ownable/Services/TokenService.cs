using System.Numerics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using ownable.Contracts;
using ownable.Models.Indexed;
using Nethereum.Web3;
using ownable.Models;
using Nethereum.Contracts;

namespace ownable.Services
{
    public class TokenService
    {
        private readonly IEnumerable<IIndexableHandler> _handlers;
        private readonly JsonSerializerOptions _options;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IEnumerable<IIndexableHandler> handlers, JsonSerializerOptions options, ILogger<TokenService> logger)
        {
            _handlers = handlers;
            _options = options;
            _logger = logger;
        }

        public async Task<string?> TryGetNameAsync<TNameFunction>(IWeb3 web3, string contractAddress, ContractFeatures features, BlockParameter atBlock) where TNameFunction : FunctionMessage, ITokenNameFunction, new()
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

        public async Task<string?> TryGetSymbolAsync<TSymbolFunction>(IWeb3 web3, string contractAddress, ContractFeatures features, BlockParameter atBlock) where TSymbolFunction : FunctionMessage, ITokenSymbolFunction, new()
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

        public async Task<string?> TryGetTokenUriAsync<TTokenUriFunction>(IWeb3 web3, string contractAddress, BigInteger tokenId, ContractFeatures features, BlockParameter atBlock) where TTokenUriFunction : FunctionMessage, ITokenUriFunction, new()
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

        public async Task<IEnumerable<Sent>> GetSentTokensAsync<TEvent>(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, CancellationToken cancellationToken, ILogger? logger = null) where TEvent : ITransferEvent, ITokenEvent, new()
        {
            logger?.LogInformation("Starting event fetch");

            var eventType = web3.Eth.GetEvent<TEvent>();
            var sentByAddress = eventType.CreateFilterInput(new object[] { account }, filterTopic2: null);
            sentByAddress.FromBlock = fromBlock;
            sentByAddress.ToBlock = toBlock;

            var sentChangeLog = await eventType.GetAllChangesAsync(sentByAddress);
            logger?.LogInformation("Fetched {Count} changes from filter", sentChangeLog.Count);

            var sent = new List<Sent>();
            foreach (var change in sentChangeLog)
            {
                var contractAddress = change.Log.Address;
                var tokenId = change.Event.GetTokenId();
                var blockNumber = change.Log.BlockNumber;

                sent.Add(new Sent
                {
                    BlockNumber = blockNumber,
                    ContractAddress = contractAddress,
                    Address = change.Event.From,
                    TokenId = new HexBigInteger(tokenId)
                });
            }

            logger?.LogInformation(JsonSerializer.Serialize(sent, _options));

            foreach (var handler in _handlers)
            {
                await handler.HandleBatchAsync(sent, cancellationToken);
            }

            return sent;
        }

        public async Task<IEnumerable<Received>> GetReceivedTokensAsync<TEvent>(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, CancellationToken cancellationToken, ILogger? logger = null) where TEvent : ITransferEvent, ITokenEvent, new()
        {
            logger?.LogInformation("Starting event fetch");

            var eventType = web3.Eth.GetEvent<TEvent>();
            var receivedByAddress = eventType.CreateFilterInput(null, new object[] { account });
            receivedByAddress.FromBlock = fromBlock;
            receivedByAddress.ToBlock = toBlock;
            
            var receivedChangeLog = await eventType.GetAllChangesAsync(receivedByAddress);
            logger?.LogInformation("Fetched {Count} changes from filter", receivedChangeLog.Count);

            var received = new List<Received>();
            foreach (var change in receivedChangeLog)
            {
                var contractAddress = change.Log.Address;
                var tokenId = change.Event.GetTokenId();
                var blockNumber = change.Log.BlockNumber;

                received.Add(new Received
                {
                    BlockNumber = blockNumber,
                    ContractAddress = contractAddress,
                    Address = change.Event.To,
                    TokenId = new HexBigInteger(tokenId)
                });
            }

            logger?.LogInformation(JsonSerializer.Serialize(received, _options));

            foreach (var handler in _handlers)
            {
                await handler.HandleBatchAsync(received, cancellationToken);
            }

            return received;
        }

        public async Task<ContractFeatures> GetContractFeaturesAsync(IWeb3 web3, string contractAddress, ERCSpecification specification)
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
}
