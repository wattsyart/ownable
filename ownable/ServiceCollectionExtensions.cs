using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ownable.Handlers;
using ownable.Indexers;
using ownable.Models;
using ownable.Processors.Images;
using ownable.Processors.Metadata;
using ownable.Services;

namespace ownable
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIndexingServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<Store>();
            services.AddSingleton<Web3Service>();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IKnownContracts, KnownContracts>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IIndexer, ERC721Indexer>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IIndexer, ERC1155Indexer>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataProcessor, HttpMetadataProcessor>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataProcessor, DataUriMetadataProcessor>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataProcessor, IpfsMetadataProcessor>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageProcessor, HttpImageProcessor>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageProcessor, DataUriImageProcessor>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageProcessor, IpfsImageProcessor>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IMetadataImageHandler, FileImageHandler>());

            services.AddSingleton<MetadataIndexer>();

            services.Configure<Web3Options>(configuration.GetSection("Web3"));
            return services;
        }
    }
}
