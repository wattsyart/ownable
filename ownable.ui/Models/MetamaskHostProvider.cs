using Nethereum.Hex.HexConvertors.Extensions;

namespace ownable.ui.Models;

public class MetamaskHostProvider : IEthereumHostProvider
{
    private readonly ILogger<MetamaskHostProvider>? _logger;
    private readonly MetamaskInterceptor _metamaskInterceptor;
    private readonly IMetamaskInterop _metamaskInterop;
        
    public string Name => "Metamask";

    public static MetamaskHostProvider? Current { get; private set; }
    public bool Available { get; private set; }
    public string? SelectedAccount { get; private set; }
    public Network Network { get; private set; }
    public bool Enabled { get; private set; }

    public event Func<string, Task>? SelectedAccountChanged;
    public event Func<Network, Task>? NetworkChanged;
    public event Func<bool, Task>? AvailabilityChanged;
    public event Func<bool, Task>? EnabledChanged;

    public MetamaskHostProvider(IMetamaskInterop metamaskInterop, ILogger<MetamaskHostProvider>? logger)
    {
        _metamaskInterop = metamaskInterop;
        _logger = logger;
        _metamaskInterceptor = new MetamaskInterceptor(_metamaskInterop, this);
        Current = this;
    }

    public async Task<bool> CheckProviderAvailabilityAsync()
    {
        var result = await _metamaskInterop.CheckMetamaskAvailability();
        await ChangeMetamaskAvailableAsync(result);
        return result;
    }

    public Task<Nethereum.Web3.Web3> GetWeb3Async()
    {
        var web3 = new Nethereum.Web3.Web3 {Client = {OverridingRequestInterceptor = _metamaskInterceptor}};
        return Task.FromResult(web3);
    }

    public async Task<string?> EnableProviderAsync()
    {
        var selectedAccount = await _metamaskInterop.EnableEthereumAsync();
        Enabled = !string.IsNullOrEmpty(selectedAccount);

        if (Enabled)
        {
            SelectedAccount = selectedAccount;
            if (SelectedAccountChanged != null)
            {
                await SelectedAccountChanged.Invoke(selectedAccount);
            }
            if (EnabledChanged != null)
            {
                await EnabledChanged.Invoke(Enabled);
            }
            return selectedAccount;
        }

        return null;
    }

    public async Task<string?> GetProviderSelectedAccountAsync()
    {
        var result = await _metamaskInterop.GetSelectedAddress();
        await ChangeSelectedAccountAsync(result);
        return result;
    }

    public async Task<Network> GetProviderNetworkAsync()
    {
        var result = await _metamaskInterop.GetNetwork();
        var network = (Network) Convert.ToInt32(result , 16);
        await ChangeNetworkAsync(network);
        return network;
    }

    public async Task<string> GetProviderEncryptionPublicKey()
    {
        var account = await _metamaskInterop.GetSelectedAddress();
        return await _metamaskInterop.GetEncryptionPublicKey(account);
    }

    public async Task<string> Encrypt(string encryptionPublicKey, string message)
    {
        return await _metamaskInterop.Encrypt(encryptionPublicKey, message);
    }

    public async Task<string> Decrypt(string encryptedMessage)
    {
        var account = await _metamaskInterop.GetSelectedAddress();
        return await _metamaskInterop.Decrypt(encryptedMessage, account);
    }

    public async Task<string> SignMessageAsync(string message)
    {
        return await _metamaskInterop.SignAsync(message.ToHexUTF8());
    }

    public async Task ChangeSelectedAccountAsync(string selectedAccount)
    {
        _logger?.LogDebug("Event: SelectedAccountChanged");
        if (SelectedAccount != selectedAccount)
        {
            SelectedAccount = selectedAccount;
            if (SelectedAccountChanged != null)
            {
                await SelectedAccountChanged.Invoke(selectedAccount);
            }
        }
    }

    public async Task ChangeNetworkAsync(Network network)
    {
        _logger?.LogDebug("Event: SelectedAccountChanged");
        if (Network != network)
        {
            Network = network;
            if (NetworkChanged != null)
            {
                await NetworkChanged.Invoke(network);
            }
        }
    }

    public async Task ChangeMetamaskAvailableAsync(bool available)
    {
        _logger?.LogDebug("Event: SelectedAccountChanged");
        Available = available;
        if (AvailabilityChanged != null)
        {
            await AvailabilityChanged.Invoke(available);
        }
    }

}