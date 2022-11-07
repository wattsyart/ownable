using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using ownable.Contracts;
using ownable.Models;

namespace ownable.Indexers;

internal sealed class ERC721Indexer : IIndexer
{
    private readonly Store _store;
    private readonly IEnumerable<IKnownContracts> _knownContracts;
    private readonly ILogger<ERC721Indexer> _logger;

    public ERC721Indexer(Store store, IEnumerable<IKnownContracts> knownContracts, ILogger<ERC721Indexer> logger)
    {
        _store = store;
        _knownContracts = knownContracts;
        _logger = logger;
    }

    public async Task IndexAddressAsync(IWeb3 web3, string address)
    {
        var @event = web3.Eth.GetEvent<ERC721.Transfer>();
        var receivedByAddress = @event.CreateFilterInput(null, new object[] { address });
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