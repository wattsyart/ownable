using ownable.dht;

namespace ownable.tests.dht
{
    public class PortScannerTests
    {
        [Fact]
        public void FindNextAvailablePort()
        {
            var scanner = new PortScanner();
            var port = scanner.GetNextAvailablePort();
            Assert.True(port > 0);
        }
    }
}
