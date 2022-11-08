window.NethereumMetamaskInterop = {

    EnableEthereum: async () => {
        try {
            ethereum.autoRefreshOnNetworkChange = true;
            const accounts = await ethereum.request({ method: "eth_requestAccounts" });
            
            ethereum.on("accountsChanged",
                function (accounts) {
                    DotNet.invokeMethodAsync("WalletConnector.Components.WalletComponent", "SelectedAccountChanged", accounts[0]);
                });

            ethereum.on("chainChanged",
                function (networkId) {
                    DotNet.invokeMethodAsync("WalletConnector.Components.WalletComponent", "NetworkChanged", networkId);
                });

            return accounts[0];
        } catch (error) {
            return null;
        }
    },

    IsMetamaskAvailable: () => {
        if (window.ethereum) return true;
        return false;
    },

    GetSelectedAddress: () => {
        return ethereum.selectedAddress;
    },

    GetNetwork: async () => {
        return await ethereum.request({ method: "eth_chainId" });
    },

    Request: async (message) => {
        const parsedMessage = JSON.parse(message);
        try {
            const response = await ethereum.request(parsedMessage);
            let rpcResponse = {
                jsonrpc: "2.0",
                result: response,
                id: parsedMessage.id,
                error: null
            }
            return JSON.stringify(rpcResponse);
        } catch (e) {
            let rpcResponseError = {
                jsonrpc: "2.0",
                id: parsedMessage.id,
                error: e
            }
            return JSON.stringify(rpcResponseError);
        }
    },

    Send: async (message) => {
        return new Promise(function (resolve, reject) {
            ethereum.sendAsync(JSON.parse(message), function (error, result) {
                resolve(JSON.stringify(result));
            });
        });
    },

    Sign: async (utf8HexMsg) => {
        return new Promise(function (resolve, reject) {
            const from = ethereum.selectedAddress;
            const params = [utf8HexMsg, from];
            const method = "personal_sign";
            ethereum.sendAsync({
                method,
                params,
                from,
            }, function (error, result) {
                if (error) {
                    reject(error);
                } else {
                    resolve(JSON.stringify(result.result));
                }
            });
        });
    },

    GetEncryptionPublicKey: async (account) => {
        return new Promise(function (resolve, reject) {
            const params = [account];
            const method = "eth_getEncryptionPublicKey";
            ethereum.sendAsync({
                method,
                params
            }, function (error, result) {
                if (error) {
                    reject(error);
                } else {
                    resolve(JSON.stringify(result.result));
                }
            });
        });
    },
    
    Decrypt: async (encryptedMessage, account) => {
        return new Promise(function (resolve, reject) {
            const params = [encryptedMessage, account];
            const method = "eth_decrypt";
            ethereum.sendAsync({
                method,
                params
            }, function (error, result) {
                if (error) {
                    reject(error);
                } else {
                    resolve(JSON.stringify(result.result));
                }
            });
        });
    }
}