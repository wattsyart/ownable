using NetMQ;
using NetMQ.Sockets;

namespace ownable.dht;

public sealed class SocketClient : IDisposable
{
    private readonly RequestSocket _socket;

    public SocketClient(string host, int port)
    {
        _socket = new RequestSocket($">tcp://{host}:{port}");
    }

    public void Dispose()
    {
        _socket.Dispose();
    }

    public bool Send(string message)
    {
        return _socket.TrySendFrame(TimeSpan.FromSeconds(1), message);
    }
}