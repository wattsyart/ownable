using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using ownable.Contracts;
using ownable.Models.Indexed;
using Nethereum.Web3;

namespace ownable.Services
{
    public class EventService
    {
        public async Task<IEnumerable<Received>> GetReceivedTokensAsync<TEvent>(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, ILogger? logger = null) where TEvent : ITransferEvent, ITokenEvent, new()
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

            return received;
        }
    }
}
