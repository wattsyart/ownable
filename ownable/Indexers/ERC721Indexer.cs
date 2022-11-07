using System.Numerics;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using ownable.Contracts;
using ownable.Indexers.Metadata;
using ownable.Models;

namespace ownable.Indexers;

internal sealed class ERC721Indexer : IIndexer
{
    private readonly Store _store;
    private readonly IEnumerable<IKnownContracts> _knownContracts;
    private readonly IEnumerable<IMetadataProcessor> _metadataProcessors;
    private readonly IEnumerable<IMetadataIndexer> _metadataIndexers;
    private readonly ILogger<ERC721Indexer> _logger;

    public ERC721Indexer(Store store, IEnumerable<IKnownContracts> knownContracts, IEnumerable<IMetadataProcessor> metadataProcessors, IEnumerable<IMetadataIndexer> metadataIndexers,  ILogger<ERC721Indexer> logger)
    {
        _store = store;
        _knownContracts = knownContracts;
        _metadataProcessors = metadataProcessors;
        _metadataIndexers = metadataIndexers;
        _logger = logger;
    }

    public async Task IndexAddressAsync(IWeb3 web3, string address, CancellationToken cancellationToken)
    {
        var @event = web3.Eth.GetEvent<ERC721.Transfer>();
        var receivedByAddress = @event.CreateFilterInput(null, new object[] { address });
        var changes = await @event.GetAllChangesAsync(receivedByAddress);

        foreach (var change in changes)
        {
            var contractAddress = change.Log.Address;
            var tokenId = change.Event.GetTokenId();

            await IndexContractAddress(web3, contractAddress, tokenId, cancellationToken);
        }
    }

    private async Task IndexContractAddress(IWeb3 web3, string contractAddress, BigInteger tokenId, CancellationToken cancellationToken)
    {
        foreach (var knownContracts in _knownContracts)
        {
            if (!knownContracts.TryGetContract(contractAddress, out var contract) || contract == null)
                continue;

            _store.Index(contract);
            return;
        }

        var supportsInterfaceQuery = web3.Eth.GetContractQueryHandler<ERC165.SupportsInterfaceFunction>();

        var supportsErc721 = await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction { InterfaceId = InterfaceIds.ERC721 });
        var supportsMetadata = await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction {InterfaceId = InterfaceIds.ERC721Metadata });
        
        if (supportsErc721)
        {
            var contract = new Contract
            {
                Address = contractAddress,
                Type = "ERC721"
            };

            var supportsName = supportsMetadata || await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction { InterfaceId = InterfaceIds.Name });
            if (supportsName)
            {
                try
                {
                    var nameQuery = web3.Eth.GetContractQueryHandler<ERC721.NameFunction>();
                    var name = await nameQuery.QueryAsync<string>(contractAddress, new ERC721.NameFunction());
                    contract.Name = name;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Contract Address {ContractAddress} does not support token name", contractAddress);
                }
            }

            var supportsSymbol = supportsMetadata || await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction { InterfaceId = InterfaceIds.Symbol });
            if (supportsSymbol)
            {
                try
                {
                    var symbolQuery = web3.Eth.GetContractQueryHandler<ERC721.SymbolFunction>();
                    var symbol = await symbolQuery.QueryAsync<string>(contractAddress, new ERC721.SymbolFunction());
                    contract.Symbol = symbol;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Contract Address {ContractAddress} does not support token symbol", contractAddress);
                }
            }

            var supportsTokenUri = supportsMetadata || await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction { InterfaceId = InterfaceIds.TokenURI });
            if (supportsTokenUri)
            {
                try
                {
                    var tokenUriQuery = web3.Eth.GetContractQueryHandler< ERC721.TokenURIFunction>();
                    var tokenUri = await tokenUriQuery.QueryAsync<string>(contractAddress, new ERC721.TokenURIFunction { TokenId = tokenId });

                    foreach (var processor in _metadataProcessors)
                    {
                        if (!processor.CanProcess(tokenUri))
                            continue;

                        var metadata = await processor.ProcessAsync(tokenUri, cancellationToken);
                        if (metadata == null)
                        {
                            _logger.LogWarning("Processor {ProcessorName} failed to process metadata, when it reported it was capable", processor.GetType().Name);
                        }
                        else
                        {
                            foreach (var indexer in _metadataIndexers)
                                await indexer.Index(metadata);

                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Contract Address {ContractAddress} does not support token URI", contractAddress);
                }
            }

            try
            {
                _store.Index(contract);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to index contract with address {ContractAddress}", contractAddress);
            }
        }
    }
}