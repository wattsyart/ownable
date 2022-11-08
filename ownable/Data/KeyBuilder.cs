using System.Reflection;

#if !PLAINSTORE
using WyHash;
#else
using System.Text;
#endif

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
            if (!KeyPrefixes.TryGetValue((type, key), out var prefix))
                KeyPrefixes.Add((type, key), prefix = IdPrefix(type).Concat(KeyTag(key)));
            return prefix;
        }
       
        public static ReadOnlySpan<byte> IndexKey(PropertyInfo property, object target, byte[] id)
        {
            var type = target.GetType();
            var value = property.GetValue(target);
            value ??= "";
            var valueString = value.ToString();
            return IndexKey(type, property.Name, valueString, id);
        }
        
        public static ReadOnlySpan<byte> IndexKey(Type type, string key, string? value, byte[] id)
        {
            var indexKey = KeyLookup(type, key, value)
                .Concat(IdPrefix(type))
                .Concat(id);

            return indexKey;
        }

        public static ReadOnlySpan<byte> KeyLookup(Type type, string key, string? value) => KeyPrefix(type, key).Concat(ValueTag(value));

        private static ReadOnlySpan<byte> ValueTag(ReadOnlySpan<char> value) => Hash(value);

        private static ReadOnlySpan<byte> IdTag(MemberInfo type) => Hash(type.Name.AsSpan());

        private static ReadOnlySpan<byte> KeyTag(string key) => Hash(key.AsSpan());

        private static ReadOnlySpan<byte> Hash(ReadOnlySpan<char> value)
        {
#if PLAINSTORE
            return Encoding.UTF8.GetBytes(new string(value).ToUpperInvariant());
#else
            var buffer = new char[value.Length];
            value.ToUpperInvariant(buffer);
            var hash = new byte[sizeof(ulong)];
            if (!BitConverter.TryWriteBytes(hash, WyHash64.ComputeHash64(buffer)))
                throw new InvalidOperationException("Failed to hash string value");
            return hash;
#endif
        }
    }
}
