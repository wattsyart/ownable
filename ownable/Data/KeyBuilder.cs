using System.Reflection;
using WyHash;

namespace ownable.Data
{
    public static class KeyBuilder
    {
        private static readonly Dictionary<Type, byte[]> IdPrefixes = new();
        private static readonly Dictionary<(Type, string), byte[]> KeyPrefixes = new();

        public static ReadOnlySpan<byte> IdPrefix(Type type)
        {
            if (!IdPrefixes.TryGetValue(type, out var prefix))
                IdPrefixes.Add(type, prefix = IdTag(type).ToArray());
            return prefix;
        }

        public static ReadOnlySpan<byte> KeyPrefix(Type type, string key)
        {
            key = key.ToUpperInvariant();
            if (!KeyPrefixes.TryGetValue((type, key), out var prefix))
                KeyPrefixes.Add((type, key), prefix = IdPrefix(type).Concat(KeyTag(key)));
            return prefix;
        }
       
        public static ReadOnlySpan<byte> IndexKey(PropertyInfo property, object target, byte[] id)
        {
            var type = property.DeclaringType ?? target.GetType();
            var value = property.GetValue(target);
            return IndexKey(type, property.Name, value!.ToString(), id);
        }
        
        public static ReadOnlySpan<byte> IndexKey(Type type, string key, string? value, byte[] id)
        {
            var indexKey = LookupKey(type, key, value)
                .Concat(IdPrefix(type))
                .Concat(id);

            return indexKey;
        }

        public static ReadOnlySpan<byte> LookupKey(Type type, string key, string? value)
        {
            var lookupKey = KeyPrefix(type, key)
                .Concat(ValueTag(value))
                .Concat(':');

            return lookupKey;
        }

        private static ReadOnlySpan<byte> ValueTag(ReadOnlySpan<char> value) => Hash(value);

        private static ReadOnlySpan<byte> IdTag(MemberInfo type) => Hash(type.Name.AsSpan());

        private static ReadOnlySpan<byte> KeyTag(string key) => Hash(key.AsSpan());

        private static ReadOnlySpan<byte> Hash(ReadOnlySpan<char> value)
        {
            var buffer = new char[value.Length];
            value.ToUpperInvariant(buffer);
            var hash = new byte[sizeof(ulong)];
            if (!BitConverter.TryWriteBytes(hash, WyHash64.ComputeHash64(buffer)))
                throw new InvalidOperationException("Failed to hash string value");
            return hash;
        }
    }
}
