using System.Text;
using Microsoft.Extensions.Logging;

namespace ownable.dht;

public sealed class KademliaServer : IKademliaService, IDisposable
{
    private readonly KademliaOptions _options;
    private readonly ILogger<KademliaServer> _logger;

    private SocketServer? _server;

    public KademliaServer(KademliaOptions options, ILogger<KademliaServer> logger)
    {
        _options = options;
        _logger = logger;
    }

    public void Start()
    {
        _server ??= new SocketServer(_options.Port, _options.Encoding);
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
        if (message.StartsWith(Messages.Ping))
        {
            if (Ping())
            {
                return "PONG";
            }
        }
        else if (message.StartsWith(Messages.Store))
        {
            var tokens = message.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var key = _options.Encoding.GetBytes(tokens[1]);
            var value = _options.Encoding.GetBytes(tokens[2]);
            if (Store(key, value))
            {
                return "OK";
            }
        }
        else if (message.StartsWith(Messages.FindNode))
        {
            return "OK";
        }
        else if (message.StartsWith(Messages.FindValue))
        {
            var tokens = message.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var result = FindValue(_options.Encoding.GetBytes(tokens[1]));
            if (result.Item1.HasValue)
            {
                var span = result.Item1.Value.AsSpan();
                return _options.Encoding.GetString(span);
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
        return _options.Store.TryPut(key, value);
    }

    public List<KademliaNode> FindNode(ReadOnlySpan<byte> key)
    {
        return new List<KademliaNode>();
    }

    public (SpanValue?, List<KademliaNode>) FindValue(ReadOnlySpan<byte> key)
    {
        if (_options.Store.TryGet(key, out var value))
        {
            unsafe
            {
                fixed (byte* ptr = value)
                {
                    return (new SpanValue(value.Length, ptr), new List<KademliaNode>());
                }
            }
        }

        return (SpanValue.Empty, new List<KademliaNode>());
    }

    public void Dispose()
    {
        _server?.Dispose();
    }
}