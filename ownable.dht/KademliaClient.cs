using System.Text;
using Microsoft.Extensions.Logging;

namespace ownable.dht;

public sealed class KademliaClient : IKademliaService, IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly ILogger<KademliaClient> _logger;
    
    private SocketClient? _client;
    private readonly INonceProvider _nonceProvider;
    private readonly List<KademliaServer> _servers;

    public ConnectionState ConnectionState { get; private set; }

    public KademliaClient(string host, int port, ILogger<KademliaClient> logger)
    {
        _host = host;
        _port = port;
        _logger = logger;
        _nonceProvider = new NonceProvider();
        _servers = new List<KademliaServer>();
    }

    public void Add(KademliaServer server)
    {
        _servers.Add(server);
    }

    public void Connect()
    {
        _client ??= new SocketClient(_host, _port);
        ConnectionState = ConnectionState.Connected;
    }

    public void Disconnect()
    {
        _client?.Dispose();
        ConnectionState = ConnectionState.Disconnected;
    }
    
    public void Dispose()
    {
        _client?.Dispose();
    }

    public bool Ping()
    {
        if(!_client!.TrySend($"PING {_nonceProvider.Next()}"))
            return false;
        
        var response = GetResponse(_client);
        return response == "PONG";
    }

    public bool Store(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        var request = $"STORE {Encoding.UTF8.GetString(key)} {Encoding.UTF8.GetString(value)} {_nonceProvider.Next()}";
        var sent = _client!.TrySend(request);
        var response = GetResponse(_client);
        return sent;
    }

    public List<KademliaNode> FindNode(ReadOnlySpan<byte> key)
    {
        var request = $"FIND_NODE {Encoding.UTF8.GetString(key)} {_nonceProvider.Next()}";
        var sent = _client!.TrySend(request);
        var response = GetResponse(_client);
        return new List<KademliaNode>();
    }

    public (byte[]?, List<KademliaNode>) FindValue(ReadOnlySpan<byte> key)
    {
        var sent = _client!.TrySend($"FIND_VALUE {Encoding.UTF8.GetString(key)} {_nonceProvider.Next()}");
        var response = GetResponse(_client);
        var nodes = new List<KademliaNode>();
        if (!string.IsNullOrWhiteSpace(response))
        {
            var payload = Encoding.UTF8.GetBytes(response);
            return (payload, nodes);
        }
        return (Array.Empty<byte>(), nodes);
    }

    private string? GetResponse(SocketClient client)
    {
        var server = _servers.First();
        server.TryProcessRequest();
        client.TryReceive(out var response);
        return response;
    }
}