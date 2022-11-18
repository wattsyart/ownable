namespace ownable.dht;

public ref struct Id
{
    public ReadOnlySpan<byte> Value;

    public Id(ReadOnlySpan<byte> frame)
    {
        Value = frame[..20];
    }
}