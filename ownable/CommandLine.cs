using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace ownable
{
    public static class CommandLine
    {
        public const string PrefixVerbose = "--";

        private static readonly Dictionary<string, Func<IConfiguration, Queue<string>, IConfiguration>> Commands = new(StringComparer.OrdinalIgnoreCase);

        static CommandLine()
        {
            AddCommand("stat", StatOptions);
            AddCommand("get", BuiltIn.GetOwnedTokens);
        }

        public static void AddCommand(string commandName, Func<IConfiguration, Queue<string>, IConfiguration> commandFunc)
        {
            Commands[commandName] = commandFunc;
        }

        public static void ProcessArguments(ref IConfiguration configuration, string[] args)
        {
            var arguments = new Queue<string>(args);

            while (arguments.Count > 0)
            {
                var commandName = arguments.Dequeue()
                    .TrimStart(PrefixVerbose.ToCharArray())
                    ;

                if (Commands.TryGetValue(commandName, out var commandFunc))
                {
                    configuration = commandFunc(configuration, arguments);
                }
            }
        }

        private static IConfiguration StatOptions(IConfiguration configuration, Queue<string> arguments)
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
}
