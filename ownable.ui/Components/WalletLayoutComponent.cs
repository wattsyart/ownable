using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ownable.ui.Models;
using Blazored.LocalStorage;

namespace ownable.ui.Components;

public abstract class WalletLayoutComponent : LayoutComponentBase, IDisposable
{
    [Inject]
    public HttpClient Http { get; set; } = null!;

    [Inject]
    public NavigationManager Nav { get; set; } = null!;

    [Inject]
    public IJSRuntime Js { get; set; } = null!;

    [Inject]
    public IEthereumHostProvider HostProvider { get; set; } = null!;

    [Inject]
    public NethereumAuthenticator Authenticator { get; set; } = null!;

    [Inject]
    public ILocalStorageService LocalStorage { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        HostProvider.AvailabilityChanged += AvailabilityChanged;
        HostProvider.EnabledChanged += EnabledChanged;
        HostProvider.SelectedAccountChanged += SelectedAccountChanged;
        HostProvider.NetworkChanged += NetworkChanged;
    }

    [Inject]
    public ILogger<WalletLayoutComponent>? Logger { get; set; }


    private Task EnabledChanged(bool enabled)
    {
        Logger?.LogDebug("WalletLayoutComponent: received EnabledChanged ({Enabled})", enabled);
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task AvailabilityChanged(bool available)
    {
        Logger?.LogDebug("WalletLayoutComponent: received AvailabilityChanged ({Available})", available);
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task SelectedAccountChanged(string account)
    {
        Logger?.LogDebug("WalletLayoutComponent: received SelectedAccountChanged ({Account})", account);
        await TryAddHttpAuthenticationAsync(account);
        StateHasChanged();
    }

    protected async Task TryAddHttpAuthenticationAsync(string account)
    {
        if (await LocalStorage.ContainKeyAsync($"JWT:{account}"))
        {
            var token = await LocalStorage.GetItemAsStringAsync($"JWT:{account}");
            if (!string.IsNullOrWhiteSpace(token))
            {
                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    token.Replace("\"", ""));
            }
        }
    }

    private Task NetworkChanged(Network network)
    {
        Logger?.LogDebug("WalletLayoutComponent: received NetworkChanged ({Network})", network);
        StateHasChanged();
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        HostProvider.AvailabilityChanged -= AvailabilityChanged;
        HostProvider.EnabledChanged -= EnabledChanged;
        HostProvider.SelectedAccountChanged -= SelectedAccountChanged;
        HostProvider.NetworkChanged -= NetworkChanged;
    }
}