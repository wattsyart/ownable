using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace ownable.dht;

public sealed class SocketServer : IDisposable
{
    private readonly ResponseSocket _socket;

    public SocketServer(int port)
    {
        _socket = new ResponseSocket($"@tcp://*:{port}");
    }

    public void Dispose()
    {
        _socket.Dispose();
    }

    public string? Receive()
    {
        _socket.TryReceiveFrameString(TimeSpan.FromSeconds(1), Encoding.UTF8, out var message);
        return message;
    }
}