using Microsoft.Extensions.Options;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using ownable.Contracts;
using ownable.Models;

namespace ownable.Services;

internal sealed class Web3Service
{
    private static readonly byte[] Erc721InterfaceId = "0x80ac58cd".HexToByteArray();

    private readonly Store _store;
    private readonly IOptionsSnapshot<Web3Options> _options;
    

    public Web3Service(Store store, IOptionsSnapshot<Web3Options> options)
    {
        _store = store;
        _options = options;
    }

    public async Task IndexAddressAsync(string address)
    {
        var uri = new Uri(_options.Value.RpcUrl!);
        var client = new RpcClient(uri);
        var web3 = new Web3(client);

        var @event = web3.Eth.GetEvent<ERC721.Transfer>();
        var receiver = @event.CreateFilterInput(null, new object[] { address });
        var changes = await @event.GetAllChangesAsync(receiver);

        foreach (var change in changes)
        {
            var contractAddress = change.Log.Address;
            var queryHandler = web3.Eth.GetContractQueryHandler<ERC721.SupportsInterfaceFunction>();
            var supportsInterface = await queryHandler.QueryAsync<bool>(contractAddress, new ERC721.SupportsInterfaceFunction {InterfaceId = Erc721InterfaceId});
                
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