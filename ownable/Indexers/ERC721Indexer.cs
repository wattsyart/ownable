using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using ownable.Contracts;
using ownable.Models;

namespace ownable.Indexers;

internal sealed class ERC721Indexer : IIndexer
{
    private static readonly byte[] Erc721InterfaceId = "0x80ac58cd".HexToByteArray();

    private readonly Store _store;

    public ERC721Indexer(Store store)
    {
        _store = store;
    }

    public async Task IndexAddressAsync(Web3 web3, string address)
    {
        var @event = web3.Eth.GetEvent<ERC721.Transfer>();
        var receiver = @event.CreateFilterInput(null, new object[] { address });
        var changes = await @event.GetAllChangesAsync(receiver);

        foreach (var change in changes)
        {
            var contractAddress = change.Log.Address;
            var queryHandler = web3.Eth.GetContractQueryHandler<ERC721.SupportsInterfaceFunction>();
            var supportsInterface = await queryHandler.QueryAsync<bool>(contractAddress, new ERC721.SupportsInterfaceFunction { InterfaceId = Erc721InterfaceId });

            if (supportsInterface)
            {
                var contract = new Contract();
                contract.Address = contractAddress;
                contract.Type = "ERC721";
                _store.Index(contract);
            }
        }
    }
}