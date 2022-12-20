using NetMQ;
using NetMQ.Sockets;

namespace ownable.dht;

public sealed class SocketClient : IDisposable
{
    private readonly RequestSocket _socket;
    private readonly TimeSpan _timeout;

    public SocketClient(string host, int port)
    {
        _socket = new RequestSocket($">tcp://{host}:{port}");
        _timeout = TimeSpan.FromSeconds(1);
    }

    public void Dispose()
    {
        _socket.Dispose();
    }

    public bool TrySend(string request)
    {
        return _socket.TrySendFrame(_timeout, request);
    }

    public bool TryReceive(out string? response)
    {
        return _socket.TryReceiveFrameString(_timeout, out response);
    }
}