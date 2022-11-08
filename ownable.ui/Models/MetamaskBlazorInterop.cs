using Microsoft.JSInterop;
using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;

namespace ownable.ui.Models;

public class MetamaskBlazorInterop : IMetamaskInterop
{
    private readonly IJSRuntime _jsRuntime;

    public MetamaskBlazorInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask<string> EnableEthereumAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.EnableEthereum");
    }

    public async ValueTask<bool> CheckMetamaskAvailability()
    {
        return await _jsRuntime.InvokeAsync<bool>("NethereumMetamaskInterop.IsMetamaskAvailable");
    }

    public async ValueTask<RpcResponseMessage> SendAsync(RpcRequestMessage rpcRequestMessage)
    {
        var response = await _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.Request", JsonConvert.SerializeObject(rpcRequestMessage));
        return JsonConvert.DeserializeObject<RpcResponseMessage>(response);
    }

    public async ValueTask<RpcResponseMessage> SendTransactionAsync(MetamaskRpcRequestMessage rpcRequestMessage)
    {
        var response = await _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.Request", JsonConvert.SerializeObject(rpcRequestMessage));
        return JsonConvert.DeserializeObject<RpcResponseMessage>(response);
    }

    public async ValueTask<string> SignAsync(string utf8Hex)
    {
        var result = await  _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.Sign", utf8Hex);
        return result.Trim('"');
    }

    public async ValueTask<string> GetSelectedAddress()
    {
        return await _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.GetSelectedAddress");
    }

    public async ValueTask<string> GetNetwork()
    {
        return await _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.GetNetwork");
    }

    public async ValueTask<string> GetEncryptionPublicKey(string account)
    {
        var result = await  _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.GetEncryptionPublicKey", account);
        return result.Trim('"');
    }

    public async ValueTask<string> Encrypt(string encryptionPublicKey, string message)
    {
        var result = await  _jsRuntime.InvokeAsync<string>("NethereumMetamaskEncryptInterop.Encrypt", encryptionPublicKey, message);
        return result.Trim('"');
    }

    public async ValueTask<string> Decrypt(string encryptedMessage, string account)
    {
        var result = await _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.Decrypt", encryptedMessage, account);
        return result.Trim('"');
    }

    [JSInvokable]
    public static async Task MetamaskAvailableChanged(bool available)
    {
        await MetamaskHostProvider.Current!.ChangeMetamaskAvailableAsync(available);
    }

    [JSInvokable]
    public static async Task SelectedAccountChanged(string selectedAccount)
    {
        await MetamaskHostProvider.Current!.ChangeSelectedAccountAsync(selectedAccount);
    }

    [JSInvokable]
    public static async Task NetworkChanged(string networkId)
    {
        var network = (Network) Convert.ToInt32(networkId, 16);
        await MetamaskHostProvider.Current!.ChangeNetworkAsync(network);
    }
}