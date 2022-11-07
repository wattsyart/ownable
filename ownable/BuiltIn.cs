using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ownable.Services;

namespace ownable
{
    internal class BuiltIn
    {
        public static IConfiguration IndexAddress(IConfiguration configuration, IServiceProvider serviceProvider, Queue<string> arguments)
        {
            if (arguments.EndOfSubArguments())
            {
                Console.Error.WriteLine("no address specified");
            }
            else
            {
                var address = arguments.Dequeue();
                var service = serviceProvider.GetRequiredService<Web3Service>();
                service.IndexAddressAsync(address, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return configuration;
        }
    }
}