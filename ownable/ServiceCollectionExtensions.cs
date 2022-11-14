using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ownable.Data;
using ownable.Handlers;
using ownable.Indexers;
using ownable.Models;
using ownable.Processors.Images;
using ownable.Processors.Metadata;
using ownable.Serialization.Converters;
using ownable.Services;

namespace ownable;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientIndexingServices(this IServiceCollection services)
    {
        AddCoreServices(services);

        services.AddScoped<TokenService>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexableHandler, ClientIndexableHandler>());

        AddProcessors(services);
        AddHandlers(services);

        return services;
    }

    public static IServiceCollection AddServerIndexingServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddCoreServices(services);

        services.Configure<Web3Options>(configuration.GetSection("Web3"));
        services.Configure<IpfsOptions>(configuration.GetSection("Ipfs"));

        services.AddSingleton<Store>();
        services.AddSingleton<IndexService>();
        services.AddSingleton<TokenService>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IIndexableHandler, ServerIndexableHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IKnownContracts, KnownContracts>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IBlockIndexer, ERC721Indexer>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IBlockIndexer, ERC1155Indexer>());
        services.AddSingleton<MetadataIndexer>();
        services.AddSingleton<MediaIndexer>();

        AddProcessors(services);
        AddHandlers(services);

        return services;
    }

    private static void AddProcessors(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataProcessor, HttpMetadataProcessor>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataProcessor, DataUriMetadataProcessor>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataProcessor, IpfsMetadataProcessor>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageProcessor, HttpImageProcessor>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageProcessor, DataUriImageProcessor>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageProcessor, IpfsImageProcessor>());
    }

    private static void AddHandlers(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageHandler, FileImageHandler>());
    }

    private static void AddCoreServices(IServiceCollection services)
    {
        services.AddSingleton(_ => GetJsonSerializerOptions());
        services.AddSingleton<QueryStore>();
    }

    public static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new BigIntegerConverter());
        options.Converters.Add(new EmptyStringAttributesConverter());
        return options;
    }
}