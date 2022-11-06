using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ownable.Indexers;
using ownable.Services;

namespace ownable
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<Store>();
            services.AddScoped<Web3Service>();

            services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexer, ERC721Indexer>());

            services.Configure<Web3Options>(configuration.GetSection("Web3"));
            return services;
        }
    }
}
