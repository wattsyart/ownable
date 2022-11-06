using Microsoft.Extensions.Configuration;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using ownable.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using ownable.Models;

namespace ownable
{
    internal class BuiltIn
    {
        private static readonly byte[] Erc721InterfaceId = "0x80ac58cd".HexToByteArray();

        public static IConfiguration GetOwnedTokens(IConfiguration configuration, Queue<string> arguments)
        {
            if (arguments.EndOfSubArguments())
            {
                Console.Error.WriteLine("no address specified");
            }
            else
            {
                var address = arguments.Dequeue();
                
                var store = new Store();
                var options = configuration.GetSection("Web3").Get<Web3Options>();
                var uri = new Uri(options.RpcUrl!);
                var client = new RpcClient(uri);
                var web3 = new Web3(client);

                var @event = web3.Eth.GetEvent<ERC721.Transfer>();
                var receiver = @event.CreateFilterInput(null, new object[] { address });
                var changes = @event.GetAllChangesAsync(receiver).ConfigureAwait(false).GetAwaiter().GetResult();

                foreach (var change in changes)
                {
                    var contractAddress = change.Log.Address;

                    var supportsInterface = web3.Eth.GetContractQueryHandler<ERC721.SupportsInterfaceFunction>()
                        .QueryAsync<bool>(contractAddress, new ERC721.SupportsInterfaceFunction {InterfaceId = Erc721InterfaceId})
                        .ConfigureAwait(false).GetAwaiter()
                        .GetResult();

                    if (supportsInterface)
                    {
                        var contract = new Contract();
                        contract.Address = contractAddress;
                        contract.Type = "ERC721";

                        store.Index(contract);
                    }
                }
            }

            return configuration;
        }
    }
}