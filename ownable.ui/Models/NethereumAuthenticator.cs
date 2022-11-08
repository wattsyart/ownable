namespace ownable.ui.Models;

public sealed class NethereumAuthenticator
{
    private readonly IEthereumHostProvider _host;
    private readonly HttpClient _client;

    public NethereumAuthenticator(IEthereumHostProvider host, HttpClient client)
    {
        _host = host;
        _client = client;
    }
}