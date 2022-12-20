namespace ownable.dht;

public struct SpanValue
{
    public static SpanValue Empty = new();
    internal IntPtr Length;
    internal unsafe byte* Data;
    internal unsafe SpanValue(int length, byte* data)
    {
        Length = (IntPtr) length;
        Data = data;
    }
    public unsafe ReadOnlySpan<byte> AsSpan() => new(Data, (int) Length);
}