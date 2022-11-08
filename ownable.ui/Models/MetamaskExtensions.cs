using Blazored.LocalStorage;

namespace ownable.ui.Models;

public static class MetamaskExtensions
{
    public static IServiceCollection AddMetamaskIntegration(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();
        services.AddSingleton<IMetamaskInterop, MetamaskBlazorInterop>();
        services.AddSingleton<MetamaskInterceptor>();
        services.AddSingleton<MetamaskHostProvider>();
        services.AddSingleton<IEthereumHostProvider>(r => r.GetRequiredService<MetamaskHostProvider>());
        services.AddScoped<NethereumAuthenticator>();

        return services;
    }
}