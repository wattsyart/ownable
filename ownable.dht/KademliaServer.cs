using System.Text;
using Microsoft.Extensions.Logging;
using ownable.store;

namespace ownable.dht;

public sealed class KademliaServer : IKademliaService, IDisposable
{
    private readonly int _port;
    private readonly IKeyValueStore _store;
    private readonly ILogger<KademliaServer> _logger;

    private SocketServer? _server;

    public KademliaServer(int port, IKeyValueStore store, ILogger<KademliaServer> logger)
    {
        _port = port;
        _store = store;
        _logger = logger;
    }

    public void Start()
    {
        _server ??= new SocketServer(_port);
    }

    public bool TryProcessRequest()
    {
        if (!_server!.TryReceive(out var message))
            return false;

        message = HandleClientMessage(message);

        if(string.IsNullOrWhiteSpace(message))
            return false;

        return _server!.TrySend(message);
    }

    private string? HandleClientMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return message;
        if (message.StartsWith("PING"))
        {
            if (Ping())
            {
                return "PONG";
            }
        }
        else if (message.StartsWith("STORE"))
        {
            var tokens = message.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var key = Encoding.UTF8.GetBytes(tokens[1]);
            var value = Encoding.UTF8.GetBytes(tokens[2]);
            if (Store(key, value))
            {
                return "OK";
            }
        }
        else if (message.StartsWith("FIND_NODE"))
        {
            return "OK";
        }
        else if (message.StartsWith("FIND_VALUE"))
        {
            var tokens = message.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var result = FindValue(Encoding.UTF8.GetBytes(tokens[1]));
            if (result.Item1 is {Length: > 0})
            {
                return Encoding.UTF8.GetString(result.Item1);
            }
        }

        return message;
    }

    public bool Ping()
    {
        return true;
    }

    public bool Store(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        return _store.TryPut(key, value);
    }

    public List<KademliaNode> FindNode(ReadOnlySpan<byte> key)
    {
        return new List<KademliaNode>();
    }

    public (byte[]?, List<KademliaNode>) FindValue(ReadOnlySpan<byte> key)
    {
        if (_store.TryGet(key, out var value))
        {
            return (value.ToArray(), new List<KademliaNode>());
        }

        return (Array.Empty<byte>(), new List<KademliaNode>());
    }

    public void Dispose()
    {
        _server?.Dispose();
    }
}