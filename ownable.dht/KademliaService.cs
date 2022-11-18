namespace ownable.dht
{
    public sealed class KademliaService
    {
        public void Ping()
        {
            
        }

        public void Store(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
        {

        }

        public List<KademliaNode> FindNode(ReadOnlySpan<byte> key)
        {
            return new List<KademliaNode>();
        }

        public (byte[]?, List<KademliaNode>) FindValue(ReadOnlySpan<byte> key)
        {
            return (null, new List<KademliaNode>());
        }
    }
}
