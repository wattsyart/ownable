using System.Numerics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using ownable.Models;
using ownable.Services;

namespace ownable;

internal class Commands
{
    public static IConfiguration IndexAccount(IConfiguration configuration, IServiceProvider serviceProvider, Queue<string> arguments)
    {
        if (arguments.EndOfSubArguments())
        {
            Console.Error.WriteLine("no address specified");
        }
        else
        {
            var account = arguments.Dequeue();

            var fromBlock = BlockParameter.CreateEarliest();
            if (!arguments.EndOfSubArguments())
            {
                var fromString = arguments.Dequeue();
                if (BigInteger.TryParse(fromString, out var fromValue))
                    fromBlock = new BlockParameter(new HexBigInteger(fromValue));
            }

            var toBlock = BlockParameter.CreateLatest();
            if (!arguments.EndOfSubArguments())
            {
                var toString = arguments.Dequeue();
                if (BigInteger.TryParse(toString, out var toValue))
                    toBlock = new BlockParameter(new HexBigInteger(toValue));
            }

            var scope = IndexScope.All;
            var service = serviceProvider.GetRequiredService<IndexService>();
            service.IndexAccountAsync(account, fromBlock, toBlock, scope, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        return configuration;
    }

    public static IConfiguration IndexCollection(IConfiguration configuration, IServiceProvider serviceProvider, Queue<string> arguments)
    {
        if (arguments.EndOfSubArguments())
        {
            Console.Error.WriteLine("no address specified");
        }
        else
        {
            var account = arguments.Dequeue();

            var fromBlock = BlockParameter.CreateEarliest();
            if (!arguments.EndOfSubArguments())
            {
                var fromString = arguments.Dequeue();
                if (BigInteger.TryParse(fromString, out var fromValue))
                    fromBlock = new BlockParameter(new HexBigInteger(fromValue));
            }

            var toBlock = BlockParameter.CreateLatest();
            if (!arguments.EndOfSubArguments())
            {
                var toString = arguments.Dequeue();
                if (BigInteger.TryParse(toString, out var toValue))
                    toBlock = new BlockParameter(new HexBigInteger(toValue));
            }

            var scope = IndexScope.All;
            var service = serviceProvider.GetRequiredService<IndexService>();
            service.IndexCollectionAsync(account, fromBlock, toBlock, scope, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        return configuration;
    }
}