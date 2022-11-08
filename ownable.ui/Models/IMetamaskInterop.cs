using Nethereum.JsonRpc.Client.RpcMessages;

namespace ownable.ui.Models;

public interface IMetamaskInterop
{
    ValueTask<string> EnableEthereumAsync();
    ValueTask<bool> CheckMetamaskAvailability();
    ValueTask<string> GetSelectedAddress();
    ValueTask<string> GetNetwork();
    ValueTask<RpcResponseMessage> SendAsync(RpcRequestMessage rpcRequestMessage);
    ValueTask<RpcResponseMessage> SendTransactionAsync(MetamaskRpcRequestMessage rpcRequestMessage);
    ValueTask<string> SignAsync(string utf8Hex);
    ValueTask<string> GetEncryptionPublicKey(string account);
    ValueTask<string> Encrypt(string encryptionPublicKey, string message);
    ValueTask<string> Decrypt(string encryptedMessage, string account);
}