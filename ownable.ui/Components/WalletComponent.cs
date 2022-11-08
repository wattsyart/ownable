using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ownable.ui.Models;
using Blazored.LocalStorage;

namespace ownable.ui.Components;

public abstract class WalletComponent : ComponentBase, IDisposable
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

    [Inject]
    public ILogger<WalletComponent>? Logger { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        HostProvider.AvailabilityChanged += AvailabilityChanged;
        HostProvider.EnabledChanged += EnabledChanged;
        HostProvider.SelectedAccountChanged += SelectedAccountChanged;
        HostProvider.NetworkChanged += NetworkChanged;
    }

    [JSInvokable]
    private Task EnabledChanged(bool enabled)
    {
        Logger?.LogDebug("WalletComponent: received EnabledChanged ({Enabled})", enabled);
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    private Task AvailabilityChanged(bool available)
    {
        Logger?.LogDebug("WalletComponent: received AvailabilityChanged ({Available})", available);
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    private async Task SelectedAccountChanged(string account)
    {
        Logger?.LogDebug("WalletComponent: received SelectedAccountChanged ({Account})", account);
        await TryAddHttpAuthenticationAsync(account);
        StateHasChanged();
    }

    [JSInvokable]
    private Task NetworkChanged(Network network)
    {
        Logger?.LogDebug("WalletComponent: received NetworkChanged ({Network})", network);
        StateHasChanged();
        return Task.CompletedTask;
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

    public virtual void Dispose()
    {
        HostProvider.AvailabilityChanged -= AvailabilityChanged;
        HostProvider.EnabledChanged -= EnabledChanged;
        HostProvider.SelectedAccountChanged -= SelectedAccountChanged;
        HostProvider.NetworkChanged -= NetworkChanged;

        GC.SuppressFinalize(this);
    }
}