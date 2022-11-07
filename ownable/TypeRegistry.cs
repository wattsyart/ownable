using System.Reflection;
using System.Text;
using ownable.Models;

namespace ownable;

public sealed class TypeRegistry
{
    private readonly Dictionary<Type, string> _typeToKeyPrefix;
    private readonly Dictionary<Type, Func<object, byte[]>> _typeToToKey;
    private readonly Dictionary<Type, Func<object, byte[]>> _typeToToKeyValue;
    private readonly Dictionary<Type, Dictionary<string, Func<object, (byte[] key, byte[] value)>>> _typeToIndexed;
    private readonly Dictionary<Type, Dictionary<string, Func<object, byte[]>>> _typeToIndexPrefix;

    private readonly Dictionary<Type, Action<object, object>> _setKey;
    private readonly Dictionary<Type, Dictionary<string, Action<object, object>>> _setField;

    public TypeRegistry()
    {
        _typeToKeyPrefix = new Dictionary<Type, string>();
        _typeToToKey = new Dictionary<Type, Func<object, byte[]>>();
        _typeToToKeyValue = new Dictionary<Type, Func<object, byte[]>>();

        _typeToIndexed = new Dictionary<Type, Dictionary<string, Func<object, (byte[] key, byte[] value)>>>();
        _typeToIndexPrefix = new Dictionary<Type, Dictionary<string, Func<object, byte[]>>>();

        _setKey = new Dictionary<Type, Action<object, object>>();
        _setField = new Dictionary<Type, Dictionary<string, Action<object, object>>>();
    }

    public void Register<T>() => Register(typeof(T));

    private void Register(Type type)
    {
        PropertyInfo? keyProperty = null;
        var indexed = new List<PropertyInfo>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
        {
            if (Attribute.IsDefined(property, typeof(KeyAttribute)))
                keyProperty = property;

            if (Attribute.IsDefined(property, typeof(IndexedAttribute)))
                indexed.Add(property);
        }

        if (keyProperty == null)
            throw new InvalidOperationException("Cannot register a type with no Key property");

        _setKey[type] = (instance, value) => { keyProperty.SetValue(instance, value); };

        _typeToKeyPrefix[type] = RegisterKeyPrefix();
        _typeToToKey[type] = instance => Encoding.UTF8.GetBytes($"{_typeToKeyPrefix[type]}{keyProperty.GetValue(instance)}");
        _typeToToKeyValue[type] = instance => Encoding.UTF8.GetBytes(keyProperty.GetValue(instance)?.ToString() ?? throw new InvalidOperationException());

        foreach (var indexedProperty in indexed)
        {
            if (!_setField.TryGetValue(type, out var fieldMap))
                _setField.Add(type, fieldMap = new Dictionary<string, Action<object, object>>());

            var field = indexedProperty.Name;

            fieldMap[field] = (instance, value) => { indexedProperty.SetValue(instance, value); };

            if (!_typeToIndexPrefix.TryGetValue(type, out var indexKeyMap))
                _typeToIndexPrefix.Add(type, indexKeyMap = new Dictionary<string, Func<object, byte[]>>());

            indexKeyMap[field] = instance => Encoding.UTF8.GetBytes($"{RegisterIndexPrefix(field)}{keyProperty.GetValue(instance)}");
                
            if (!_typeToIndexed.TryGetValue(type, out var indexValueMap))
                _typeToIndexed.Add(type, indexValueMap = new Dictionary<string, Func<object, (byte[], byte[])>>());
                
            indexValueMap[field] = instance =>
            {
                var key = indexKeyMap[field](instance);
                var value = indexedProperty.GetValue(instance);

                return (key,
                    value: value == null
                        ? Array.Empty<byte>()
                        : Encoding.UTF8.GetBytes(value.ToString() ?? throw new InvalidOperationException()));
            };
        }
    }

    private static string RegisterIndexPrefix(string field)
    {
        return $"B:I:{field}";
    }

    private static string RegisterKeyPrefix()
    {
        return "A:";
    }

    public byte[] GetKey<T>(T instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        return _typeToToKey[typeof(T)].Invoke(instance);
    }

    public byte[] GetKeyValue<T>(T instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        return _typeToToKeyValue[typeof(T)].Invoke(instance);
    }

    public IEnumerable<Func<object, (byte[], byte[])>> GetIndexed<T>(T instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        return _typeToIndexed[typeof(T)].Values;
    }

    public byte[] GetIndexKey<T>(T instance, string indexed)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        return _typeToIndexPrefix[typeof(T)][indexed](instance);
    }

    public byte[] GetKeyPrefix<T>()
    {
        return Encoding.UTF8.GetBytes(_typeToKeyPrefix[typeof(T)]);
    }

    public void SetKey<T>(T instance, object value)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        _setKey[typeof(T)](instance, value);
    }

    public IEnumerable<string> GetIndexFields<T>()
    {
        foreach (var entry in _setField[typeof(T)])
        {
            yield return entry.Key;
        }
    }
        
    public void SetField<T>(T instance, string field, object value)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        _setField[typeof(T)][field](instance, value);
    }
}