using System.Net;
using System.Net.Sockets;

namespace ownable.dht;

public sealed class PortScanner : IPortScanner
{
    public int GetNextAvailablePort()
    {
        var port = 0;
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            socket.Bind(endpoint);
            endpoint = (IPEndPoint?)socket.LocalEndPoint;
            if (endpoint != null)
                port = endpoint.Port;
        }
        finally
        {
            socket.Close();
        }
        return port;
    }
}