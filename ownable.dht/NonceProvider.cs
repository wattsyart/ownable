namespace ownable.dht;

public sealed class NonceProvider : INonceProvider
{
    private readonly XorShift _random;

    public NonceProvider()
    {
        _random = new XorShift();
    }

    public int Next()
    {
        return _random.Next();
    }
}