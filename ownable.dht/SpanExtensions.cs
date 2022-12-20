namespace ownable.dht
{
    internal static class SpanExtensions
    {
        public static SpanValue AsSpanValue(this Span<byte> value)
        {
            unsafe
            {
                fixed (byte* ptr = value)
                {
                    return new SpanValue(value.Length, ptr);
                }
            }
        }

        public static SpanValue AsSpanValue(this ReadOnlySpan<byte> value)
        {
            unsafe
            {
                fixed (byte* ptr = value)
                {
                    return new SpanValue(value.Length, ptr);
                }
            }
        }
    }
}
