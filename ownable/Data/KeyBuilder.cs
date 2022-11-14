using System.Reflection;
using System.Text;
using Combinatorics.Collections;

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

        public static IEnumerable<byte[]> IndexKeys(PropertyInfo[] properties, object target, byte[] id)
        {
            if (properties.Length == 1)
                return new List<byte[]> { IndexKey(properties[0], target, id).ToArray() };

            var type = target.GetType();
            var indexKeys = new List<byte[]>();
            var length = properties.Length;

            while (length >= 2)
                indexKeys.AddRange(IndexKeys(type, properties, target, id, length--));

            return indexKeys;
        }

        private static IEnumerable<byte[]> IndexKeys(Type type, IEnumerable<PropertyInfo> properties, object target, byte[] id, int count)
        {
            var indexKeys = new List<byte[]>();
            foreach (var combination in new Combinations<PropertyInfo>(properties, count, GenerateOption.WithoutRepetition))
            {
                var keyBuilder = new StringBuilder();
                var valueBuilder = new StringBuilder();

                foreach (var property in combination.OrderBy(x => x.Name))
                {
                    keyBuilder.Append(property.Name);

                    var value = property.GetValue(target);
                    value ??= "";

                    var valueString = value.ToString();
                    valueBuilder.Append(valueString);
                }

                var indexKeyKey = keyBuilder.ToString();
                var indexKeyValue = valueBuilder.ToString();
                var indexKey = IndexKey(type, indexKeyKey, indexKeyValue, id);
                indexKeys.Add(indexKey.ToArray());
            }

            return indexKeys;
        }

        public static ReadOnlySpan<byte> IndexKey(Type type, string key, string? value, byte[] id)
        {
            var indexKey = KeyLookup(type, key, value)
                .Concat(IdPrefix(type))
                .Concat(id);

            return indexKey;
        }

        public static ReadOnlySpan<byte> KeyLookup(Type type, string key, string? value) => KeyPrefix(type, key).Concat(ValueTag(value));

        public static ReadOnlySpan<byte> KeyLookup(Type type, string[] keys, string?[] values)
        {
            var keyBuilder = new StringBuilder();
            foreach (var key in keys.OrderBy(x => x))
                keyBuilder.Append(key);

            var valueBuilder = new StringBuilder();
            foreach (var value in values.OrderBy(x => x))
                valueBuilder.Append(value ?? "");

            return KeyPrefix(type, keyBuilder.ToString()).Concat(ValueTag(valueBuilder.ToString()));
        }

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
