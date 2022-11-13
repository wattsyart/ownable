using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ownable;

public static class CommandLine
{
    public const string PrefixVerbose = "--";

    private static readonly Dictionary<string, Func<IConfiguration, IServiceProvider, Queue<string>, IConfiguration>> Commands = new(StringComparer.OrdinalIgnoreCase);

    static CommandLine()
    {
        AddCommand("stat", StatOptions);
        AddCommand("index", ownable.Commands.IndexAccount);
    }

    public static void AddCommand(string commandName, Func<IConfiguration, IServiceProvider, Queue<string>, IConfiguration> commandFunc)
    {
        Commands[commandName] = commandFunc;
    }

    public static void ProcessArguments(ref IConfiguration configuration, IServiceCollection services, string[] args)
    {
        var arguments = new Queue<string>(args);
        var serviceProvider = services.BuildServiceProvider();

        while (arguments.Count > 0)
        {
            var commandName = arguments.Dequeue()
                    .TrimStart(PrefixVerbose.ToCharArray())
                ;

            if (Commands.TryGetValue(commandName, out var commandFunc))
            {
                configuration = commandFunc(configuration, serviceProvider, arguments);
            }
        }
    }

    private static IConfiguration StatOptions(IConfiguration configuration, IServiceProvider serviceProvider, Queue<string> arguments)
    {
        if (arguments.EndOfSubArguments())
        {
            Console.Error.WriteLine("no stat object specified");
        }
        else
        {
            var target = arguments.Dequeue();
            switch (target.ToLowerInvariant())
            {
                case "version":
                    var assemblyName = Assembly.GetExecutingAssembly().GetName();
                    Console.Out.WriteLine($"{assemblyName.Name} {assemblyName.Version}");
                    break;
                default:
                    Console.Error.WriteLine($"unrecognized stat '{target}'");
                    break;
            }
        }

        return configuration;
    }
        
    public static bool EndOfSubArguments(this Queue<string> arguments) => arguments.Count == 0 ||
                                                                          arguments.Peek().StartsWith(PrefixVerbose);
}