using ownable.store;
using System.Text;

namespace ownable.dht;

public sealed class KademliaOptions
{
    public string Host { get; set; } = "localhost";
    public IKeyValueStore Store { get; set; } = null!;
    public INonceProvider NonceProvider { get; set; } = new NonceProvider();
    public IPortScanner PortScanner { get; set; } = new PortScanner();
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    private int _port;

    public int Port
    {
        get
        {
            if (_port != 0)
                return _port;
            _port = PortScanner.GetNextAvailablePort();
            return _port;
        }
        set => _port = value;
    }

}