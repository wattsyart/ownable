using Microsoft.Extensions.Logging;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using ownable.Contracts;
using ownable.Models;
using Contract = ownable.Models.Contract;

namespace ownable.Indexers
{
    internal sealed class ERC1155Indexer : IIndexer
    {
        private readonly Store _store;
        private readonly IEnumerable<IKnownContracts> _knownContracts;
        private readonly ILogger<ERC721Indexer> _logger;

        public ERC1155Indexer(Store store, IEnumerable<IKnownContracts> knownContracts, ILogger<ERC721Indexer> logger)
        {
            _store = store;
            _knownContracts = knownContracts;
            _logger = logger;
        }

        public async Task IndexAddressAsync(IWeb3 web3, string address)
        {
            await IndexContractAddressByEvent<ERC1155.TransferSingle>(web3, address);
            await IndexContractAddressByEvent<ERC1155.TransferBatch>(web3, address);
        }

        private async Task IndexContractAddressByEvent<TEvent>(IWeb3 web3, string address) where TEvent : IEventDTO, new()
        {
            var @event = web3.Eth.GetEvent<TEvent>();
            var receivedByAddress = @event.CreateFilterInput(null, new object[] {address});
            var changes = await @event.GetAllChangesAsync(receivedByAddress);

            foreach (var change in changes)
            {
                var contractAddress = change.Log.Address;

                await IndexContractAddress(web3, contractAddress);
            }
        }

        private async Task IndexContractAddress(IWeb3 web3, string contractAddress)
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
            // var supportsMetadata = await supportsInterfaceQuery.QueryAsync<bool>(contractAddress, new ERC165.SupportsInterfaceFunction {InterfaceId = InterfaceIds.ERC1155Metadata });

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
