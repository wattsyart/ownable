namespace ownable.dht;

public interface IKademliaService
{
    public bool Ping();
    public bool Store(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value);
    public List<KademliaNode> FindNode(ReadOnlySpan<byte> key);
    public (SpanValue?, List<KademliaNode>) FindValue(ReadOnlySpan<byte> key);
}