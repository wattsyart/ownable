using Microsoft.AspNetCore.Components;
using ownable.ui.Models;

namespace ownable.ui.Components;

public abstract class WalletComponent : AppComponent, IDisposable
{
    [Inject]
    public IEthereumHostProvider HostProvider { get; set; } = null!;

    [Inject]
    public NethereumAuthenticator Authenticator { get; set; } = null!;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        HostProvider.AvailabilityChanged += AvailabilityChanged;
        HostProvider.EnabledChanged += EnabledChanged;
        HostProvider.SelectedAccountChanged += SelectedAccountChanged;
        HostProvider.NetworkChanged += NetworkChanged;
    }

    private Task EnabledChanged(bool enabled)
    {
        Logger?.LogDebug("WalletComponent: received EnabledChanged ({Enabled})", enabled);
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task AvailabilityChanged(bool available)
    {
        Logger?.LogDebug("WalletComponent: received AvailabilityChanged ({Available})", available);
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task SelectedAccountChanged(string account)
    {
        Logger?.LogDebug("WalletComponent: received SelectedAccountChanged ({Account})", account);
        await TryAddHttpAuthenticationAsync(account);
        StateHasChanged();
    }

    private Task NetworkChanged(Network network)
    {
        Logger?.LogDebug("WalletComponent: received NetworkChanged ({Network})", network);
        StateHasChanged();
        return Task.CompletedTask;
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