using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using ownable.Models;

namespace ownable.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddLmdbLogging(this ILoggingBuilder builder, string path)
    {
        builder.AddConfiguration();
        builder.Services.TryAddSingleton(r => new LightningLogStore(path));
        builder.Services.TryAddSingleton<ILogStore>(r => r.GetRequiredService<LightningLogStore>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LightningLoggerProvider>(r => new LightningLoggerProvider(r.GetRequiredService<LightningLogStore>())));
        return builder;
    }
}