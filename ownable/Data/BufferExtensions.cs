using System.Runtime.CompilerServices;

namespace ownable.Data
{
    internal static class BufferExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Concat(this byte[] left, byte[] right) => Concat(left.AsSpan(), right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Concat(this byte[] left, ReadOnlySpan<byte> right) => Concat(left.AsSpan(), right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Concat(this byte[] left, byte right) => Concat(left.AsSpan(), new[] { right });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Concat(this byte[] left, char right) => left.Concat((byte) right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Concat(this ReadOnlySpan<byte> left, ReadOnlySpan<byte> right) => left.Concat<byte>(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Concat<T>(this ReadOnlySpan<T> left, ReadOnlySpan<T> right)
        {
            var result = new T[left.Length + right.Length];
            left.CopyTo(result);
            right.CopyTo(result.AsSpan()[left.Length..]);
            return result;
        }
    }
}
