using ownable.dht;

namespace ownable.tests.dht
{
    public class ClientServerTests
    {
        [Theory]
        [InlineData("Hello, World!")]
        public void SendAndReceive(string message)
        {
            var scanner = new PortScanner();
            var port = scanner.GetNextAvailablePort();

            var client = new SocketClient("localhost", port);
            var server = new SocketServer(port);
            
            AssertRequestResponse(message, client, server);
            AssertRequestResponse(message, client, server);
        }

        private static void AssertRequestResponse(string message, SocketClient client, SocketServer server)
        {
            Assert.True(client.TrySend(message));
            Assert.True(server.TryReceive(out var request));
            Assert.Equal(message, request);

            Assert.True(server.TrySend(message));
            Assert.True(client.TryReceive(out var response));
            Assert.Equal(message, response);
        }
    }
}
