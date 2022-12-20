using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace ownable.dht;

public sealed class SocketServer : IDisposable
{
    private readonly ResponseSocket _socket;
    private readonly TimeSpan _timeout;

    public SocketServer(int port)
    {
        _socket = new ResponseSocket($"@tcp://*:{port}");
        _timeout = TimeSpan.FromSeconds(1);
    }
    
    public bool TryReceive(out string? request)
    {
        return _socket.TryReceiveFrameString(_timeout, Encoding.UTF8, out request);
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