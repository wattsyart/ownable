using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace ownable.dht;

public sealed class SocketServer : IDisposable
{
    private readonly Encoding _encoding;
    private readonly ResponseSocket _socket;
    private readonly TimeSpan _timeout;

    public SocketServer(int port, Encoding encoding)
    {
        _encoding = encoding;
        _socket = new ResponseSocket($"@tcp://*:{port}");
        _timeout = TimeSpan.FromSeconds(1);
    }
    
    public bool TryReceive(out string? request)
    {
        return _socket.TryReceiveFrameString(_timeout, _encoding, out request);
    }

    public bool TrySend(string response)
    {
        return _socket!.TrySendFrame(_timeout, response);
    }

    public void Dispose()
    {
        _socket.Dispose();
    }
}