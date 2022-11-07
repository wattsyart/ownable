using ownable.Models;
using System.Reflection;
using System.Text;

namespace ownable.Data
{
    public static class KeyBuilder
    {
        private static readonly Dictionary<Type, byte[]> IdPrefixes = new();

        public static byte[] IdPrefix(Type type)
        {
            if (!IdPrefixes.TryGetValue(type, out var prefix))
                IdPrefixes.Add(type, prefix = Encoding.UTF8.GetBytes(IdPrefixString(type)));
            return prefix;
        }

        private static string IdPrefixString(MemberInfo type) => $"{type.Name.ToUpperInvariant()}:";

        private static readonly Dictionary<(Type, string), byte[]> KeyPrefixes = new();

        public static byte[] KeyPrefix(Type type, string key)
        {
            key = key.ToUpperInvariant();
            if (!KeyPrefixes.TryGetValue((type, key), out var prefix))
                KeyPrefixes.Add((type, key), prefix = IdPrefix(type)
                    .Concat(Encoding.UTF8.GetBytes(KeyPrefixString(key))));
            return prefix;
        }

        private static string KeyPrefixString(string key) => $"{key.ToUpperInvariant()}:";
       
        public static bool IndexKey(PropertyInfo property, object target, byte[] id, out byte[]? indexKey)
        {
            if (!property.CanRead || !property.HasAttribute<IndexedAttribute>())
            {
                indexKey = default;
                return false;
            }

            var type = property.DeclaringType ?? target.GetType();
            var value = property.GetValue(target);
            indexKey = IndexKey(type, property.Name, value!.ToString(), id);
            return true;
        }

        public static byte[] IndexKey(Type type, string key, string? value, byte[] id)
        {
            var indexKey = LookupKey(type, key, value)
                .Concat(IdPrefix(type))
                .Concat(id);

            return indexKey;
        }

        public static byte[] LookupKey(Type type, string key, string? value)
        {
            var lookupKey = KeyPrefix(type, key)
                .Concat(PrepareValue(value))
                .Concat(':');

            return lookupKey;
        }

        private static ReadOnlySpan<byte> PrepareValue(ReadOnlySpan<char> value)
        {
            var buffer = new char[value.Length];
            value.ToUpperInvariant(buffer);
            var result = Encoding.UTF8.GetBytes(new string(buffer));
            return result;
        }
    }
}
