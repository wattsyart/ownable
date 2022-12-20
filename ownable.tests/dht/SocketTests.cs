using NetMQ;
using NetMQ.Sockets;
using ownable.dht;

namespace ownable.tests.dht;

public class SocketTests
{
    [Fact]
    public void SendAndReceive()
    {
        const string host = "localhost";

        var scanner = new PortScanner();
        var port = scanner.GetNextAvailablePort();

        using var server = new ResponseSocket($"@tcp://*:{port}");
        using var client = new RequestSocket($">tcp://{host}:{port}");

        client.SendFrame("Hello", more: false);
        Assert.Equal("Hello", server.ReceiveFrameString());

        server.SendFrame("World");
        Assert.Equal("World", client.ReceiveFrameString());
    }
}