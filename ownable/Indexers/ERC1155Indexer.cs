using System.Numerics;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using ownable.Contracts;
using ownable.Indexers.Metadata;
using ownable.Models;
using Contract = ownable.Models.Contract;

namespace ownable.Indexers
{
    internal sealed class ERC1155Indexer : IIndexer
    {
        private readonly Store _store;
        private readonly IEnumerable<IKnownContracts> _knownContracts;
        private readonly IEnumerable<IMetadataProcessor> _metadataProcessors;
        private readonly IEnumerable<IMetadataIndexer> _metadataIndexers;
        private readonly ILogger<ERC721Indexer> _logger;

        public ERC1155Indexer(Store store, IEnumerable<IKnownContracts> knownContracts, IEnumerable<IMetadataProcessor> metadataProcessors, IEnumerable<IMetadataIndexer> metadataIndexers, ILogger<ERC721Indexer> logger)
        {
            _store = store;
            _knownContracts = knownContracts;
            _metadataProcessors = metadataProcessors;
            _metadataIndexers = metadataIndexers;
            _logger = logger;
        }

        public async Task IndexAddressAsync(IWeb3 web3, string address, CancellationToken cancellationToken)
        {
            await IndexContractAddressByEvent<ERC1155.TransferSingle>(web3, address, cancellationToken);
            await IndexContractAddressByEvent<ERC1155.TransferBatch>(web3, address, cancellationToken);
        }

        private async Task IndexContractAddressByEvent<TEvent>(IWeb3 web3, string address,
            CancellationToken cancellationToken) where TEvent : ITokenEvent, new()
        {
            var @event = web3.Eth.GetEvent<TEvent>();
            var receivedByAddress = @event.CreateFilterInput(null, new object[] {address});
            var changes = await @event.GetAllChangesAsync(receivedByAddress);

            foreach (var change in changes)
            {
                var contractAddress = change.Log.Address;

                await IndexContractAddress(web3, contractAddress, change.Event.GetTokenId(), cancellationToken);
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
            var supportsErc1155 = await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction { InterfaceId = InterfaceIds.ERC1155 });

            // IMPORTANT: ERC1155Metadata only specifies uri(uint256), not name or symbol!
            var supportsMetadata = await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction {InterfaceId = InterfaceIds.ERC1155Metadata });

            if (supportsErc1155)
            {
                var contract = new Contract
                {
                    Address = contractAddress,
                    Type = "ERC1155"
                };

                var supportsName = await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction { InterfaceId = InterfaceIds.Name });
                if (supportsName)
                {
                    try
                    {
                        var nameQuery = web3.Eth.GetContractQueryHandler<ERC1155.NameFunction>();
                        var name = await nameQuery.QueryAsync<string>(contractAddress, new ERC1155.NameFunction());
                        contract.Name = name;
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Contract Address {ContractAddress} does not support token name", contractAddress);
                    }
                }

                var supportsSymbol = await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction { InterfaceId = InterfaceIds.Symbol });
                if (supportsSymbol)
                {
                    try
                    {
                        var symbolQuery = web3.Eth.GetContractQueryHandler<ERC1155.SymbolFunction>();
                        var symbol = await symbolQuery.QueryAsync<string>(contractAddress, new ERC1155.SymbolFunction());
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
                        var tokenUriQuery = web3.Eth.GetContractQueryHandler<ERC1155.URIFunction>();
                        var tokenUri = await tokenUriQuery.QueryAsync<string>(contractAddress, new ERC1155.URIFunction { TokenId = tokenId });

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
}
