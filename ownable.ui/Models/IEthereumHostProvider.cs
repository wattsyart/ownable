using Nethereum.Web3;

namespace ownable.ui.Models;

public interface IEthereumHostProvider
{
    string Name { get; }

    bool Available { get; }
    string? SelectedAccount { get;}
    Network Network { get; }
    bool Enabled { get; }
        
    event Func<string, Task> SelectedAccountChanged;
    event Func<Network, Task> NetworkChanged;
    event Func<bool, Task> AvailabilityChanged;
    event Func<bool, Task> EnabledChanged;

    Task<bool> CheckProviderAvailabilityAsync();
    Task<Web3> GetWeb3Async();
    Task<string?> EnableProviderAsync();
    Task<string?> GetProviderSelectedAccountAsync();
    Task<Network> GetProviderNetworkAsync();
        
    Task<string> SignMessageAsync(string message);
    Task<string> GetProviderEncryptionPublicKey();
    Task<string> Encrypt(string encryptionPublicKey, string message);
    Task<string> Decrypt(string encryptedMessage);
}