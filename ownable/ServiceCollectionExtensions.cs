using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ownable.Data;
using ownable.Handlers;
using ownable.Indexers;
using ownable.Models;
using ownable.Serialization.Converters;
//using ownable.Processors.Images;
//using ownable.Processors.Metadata;
using ownable.Services;

namespace ownable;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientIndexingServices(this IServiceCollection services)
    {
        AddIndexingServicesCore(services);
        services.AddScoped<EventService>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexableHandler, ClientIndexableHandler>());
        return services;
    }

    public static IServiceCollection AddServerIndexingServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddIndexingServicesCore(services);

        services.AddSingleton<Store>();
        services.AddSingleton<Web3Service>();
        services.AddSingleton<EventService>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IIndexableHandler, ServerIndexableHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IKnownContracts, KnownContracts>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IIndexer, ERC721Indexer>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IIndexer, ERC1155Indexer>());

        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataProcessor, HttpMetadataProcessor>());
        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataProcessor, DataUriMetadataProcessor>());
        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataProcessor, IpfsMetadataProcessor>());

        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageProcessor, HttpImageProcessor>());
        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageProcessor, DataUriImageProcessor>());
        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageProcessor, IpfsImageProcessor>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageHandler, FileImageHandler>());

        services.AddSingleton<MetadataIndexer>();

        services.Configure<Web3Options>(configuration.GetSection("Web3"));
        return services;
    }

    private static void AddIndexingServicesCore(IServiceCollection services)
    {
        services.AddSingleton(_ => GetJsonSerializerOptions());
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