using ownable.dht;

namespace ownable.tests.dht
{
    public class ClientServerTests
    {
        [Fact]
        public void SendAndReceive()
        {
            var client = new SocketClient("localhost", 9999);
            var server = new SocketServer(9999);
            
            const string sent = "Hello, World";
            client.Send(sent);
            var received = server.Receive();
            Assert.Equal(sent, received);
        }
    }
}
