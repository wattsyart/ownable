using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.RPC.Eth.DTOs;

namespace ownable.ui.Models;

public class MetamaskInterceptor : RequestInterceptor
{
    private readonly IMetamaskInterop _metamaskInterop;
    private readonly MetamaskHostProvider _metamaskHostProvider;

    public MetamaskInterceptor(IMetamaskInterop metamaskInterop, MetamaskHostProvider metamaskHostProvider)
    {
        _metamaskInterop = metamaskInterop;
        _metamaskHostProvider = metamaskHostProvider;
    }

    public override async Task<object> InterceptSendRequestAsync<T>(
        Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync, RpcRequest request,
        string? route = null)
    {
        if (request.Method == "eth_sendTransaction")
        {
            var selectedAccount = _metamaskHostProvider.SelectedAccount;
            if (string.IsNullOrWhiteSpace(selectedAccount))
                throw new InvalidOperationException("no selected account");

            var transaction = (TransactionInput)request.RawParameters[0];
            transaction.From = _metamaskHostProvider.SelectedAccount;
            request.RawParameters[0] = transaction;

            var response = await _metamaskInterop.SendAsync(new MetamaskRpcRequestMessage(request.Id, request.Method, selectedAccount,
                request.RawParameters));
            return ConvertResponse<T>(response);
        }
        else
        {
            var response = await _metamaskInterop.SendAsync(new RpcRequestMessage(request.Id,
                request.Method,
                request.RawParameters));
            return ConvertResponse<T>(response); 
        }

    }

    public override async Task<object?> InterceptSendRequestAsync<T>(
        Func<string, string, object[], Task<T>> interceptedSendRequestAsync, string method,
        string? route = null, params object[] paramList)
    {
        var selectedAccount = _metamaskHostProvider.SelectedAccount;
        if (string.IsNullOrWhiteSpace(selectedAccount))
            throw new InvalidOperationException("no selected account");

        if (method == "eth_sendTransaction")
        {
            var transaction = (TransactionInput) paramList[0];
            transaction.From = selectedAccount;
            paramList[0] = transaction;

            if (string.IsNullOrWhiteSpace(route))
                return null;

            var response = await _metamaskInterop.SendAsync(new MetamaskRpcRequestMessage(route, method, selectedAccount, paramList));
            return ConvertResponse<T>(response);
        }
        else
        {
            var response = await _metamaskInterop.SendAsync(new RpcRequestMessage(route, selectedAccount, method, paramList));
            return ConvertResponse<T>(response);
        }
          
    }
        
    protected void HandleRpcError(RpcResponseMessage response)
    {
        if (response.HasError)
            throw new RpcResponseException(new Nethereum.JsonRpc.Client.RpcError(response.Error.Code, response.Error.Message,
                response.Error.Data));
    }

    private  T ConvertResponse<T>(RpcResponseMessage response)
    {
        HandleRpcError(response);
        try
        {
            return response.GetResult<T>();
        }
        catch (FormatException formatException)
        {
            throw new RpcResponseFormatException("Invalid format found in RPC response", formatException);
        }
    }

}