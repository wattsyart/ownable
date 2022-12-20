using Microsoft.Extensions.Logging;

namespace ownable.dht;

public sealed class KademliaClient : IKademliaService, IDisposable
{
    private readonly KademliaOptions _options;
    private readonly ILogger<KademliaClient> _logger;
    
    private SocketClient? _client;
    private readonly List<KademliaServer> _servers;

    public ConnectionState ConnectionState { get; private set; }

    public KademliaClient(KademliaOptions options, ILogger<KademliaClient> logger)
    {
        _options = options;
        _logger = logger;
        _servers = new List<KademliaServer>();
    }

    public void Add(KademliaServer server)
    {
        _servers.Add(server);
    }

    public void Connect()
    {
        _client ??= new SocketClient(_options.Host, _options.Port, _options.Encoding);
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
        if(!_client!.TrySend($"{Messages.Ping} {_options.NonceProvider.Next()}"))
            return false;
        
        var response = GetResponse(_client);
        return response == "PONG";
    }

    public bool Store(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        var request = $"{Messages.Store} {_options.Encoding.GetString(key)} {_options.Encoding.GetString(value)} {_options.NonceProvider.Next()}";
        var sent = _client!.TrySend(request);
        var response = GetResponse(_client);
        return sent;
    }

    public List<KademliaNode> FindNode(ReadOnlySpan<byte> key)
    {
        var request = $"{Messages.FindNode} {_options.Encoding.GetString(key)} {_options.NonceProvider.Next()}";
        var sent = _client!.TrySend(request);
        var response = GetResponse(_client);
        return new List<KademliaNode>();
    }

    public (SpanValue?, List<KademliaNode>) FindValue(ReadOnlySpan<byte> key)
    {
        var nodes = new List<KademliaNode>();
        if (!_client!.TrySend($"{Messages.FindValue} {_options.Encoding.GetString(key)} {_options.NonceProvider.Next()}"))
            return (SpanValue.Empty, nodes);

        var response = GetResponse(_client);
        if (!string.IsNullOrWhiteSpace(response))
        {
            var payload = _options.Encoding.GetBytes(response).AsSpan();
            return (payload.AsSpanValue(), nodes);
        }

        return (SpanValue.Empty, nodes);
    }

    private string? GetResponse(SocketClient client)
    {
        var server = _servers.First();
        server.TryProcessRequest();
        client.TryReceive(out var response);
        return response;
    }
}