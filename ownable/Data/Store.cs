using System.Diagnostics;
using System.Numerics;
using LightningDB;
using System.Text;
using ownable.Models;
using ownable.Models.Indexed;
using ownable.Serialization;

namespace ownable.Data;

public class Store : IDisposable
{
    private readonly LightningEnvironment _env;
    private readonly TypeRegistry _types;
    private readonly Dictionary<Type, Func<string, object>> _stringToObject;

    public bool UseGzip { get; set; }

    // ReSharper disable once UnusedMember.Global (Reflection)
    public Store() : this("store") { }

    public Store(string path)
    {
        Path = path;

        _env = new LightningEnvironment(path, new EnvironmentConfiguration { MapSize = 10_485_760 });
        _env.MaxDatabases = 1;
        _env.Open();

        _types = new TypeRegistry();
        _types.Register<Contract>();
        _types.Register<Received>();
        _types.Register<Sent>();

        _stringToObject = new Dictionary<Type, Func<string, object>>
        {
            {typeof(string), s => s},
            {typeof(Guid), s => Guid.TryParse(s, out var value) ? value : Guid.Empty},
            {typeof(ulong), s => ulong.TryParse(s, out var value) ? value : 0UL},
            {typeof(BigInteger), s => BigInteger.TryParse(s, out var value) ? value : BigInteger.Zero}
        };
    }

    public string Path { get; set; }

    public void Save<T>(T instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        using var tx = _env.BeginTransaction();
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });

        tx.Put(db, _types.GetKey(instance), _types.GetKeyValue(instance), PutOptions.NoOverwrite);

        foreach (var indexed in _types.GetIndexed(instance))
        {
            var (key, value) = indexed(instance);
            tx.Put(db, key, value);
        }

        tx.Commit();
    }

    public IEnumerable<T> Get<T>(CancellationToken cancellationToken) where T : new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.None });
        using var cursor = tx.CreateCursor(db);

        var entries = new Dictionary<object, T>();

        {
            var keyPrefix = _types.GetKeyPrefix<T>();
            var sr = cursor.SetRange(keyPrefix);
            if (sr != MDBResultCode.Success)
                return entries.Values;

            var (r, k, v) = cursor.GetCurrent();

            while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
            {
                if (!k.AsSpan().StartsWith(keyPrefix))
                    break;

                var index = v.AsSpan();
                if (index.Length == 0)
                    break;

                var keyType = _types.GetKeyType<T>();
                var key = _stringToObject[keyType](Encoding.UTF8.GetString(index));
                var value = new T();

                _types.SetKey(value, key);
                entries.Add(key, value);

                r = cursor.Next();
                if (r == MDBResultCode.Success)
                    (r, k, v) = cursor.GetCurrent();
            }
        }

        foreach (var entry in entries)
        {
            foreach (var field in _types.GetIndexFields<T>())
            {
                var indexKey = _types.GetIndexKey(entry.Value, field);
                var sr = cursor.SetRange(indexKey);
                if (sr != MDBResultCode.Success)
                    return entries.Values;

                var (r, k, v) = cursor.GetCurrent();

                while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
                {
                    if (!k.AsSpan().StartsWith(indexKey))
                        break;

                    var index = v.AsSpan();
                    if (index.Length == 0)
                        break;

                    var fieldType = _types.GetFieldType<T>(field);
                    var fieldRawValue = Encoding.UTF8.GetString(index);
                    var fieldValue = _stringToObject[fieldType](fieldRawValue);

                    _types.SetField(entry.Value, field, fieldValue);

                    r = cursor.Next();
                    if (r == MDBResultCode.Success)
                        (r, k, v) = cursor.GetCurrent();
                }
            }
        }

        return entries.Values;
    }

    private const ushort MaxKeySizeBytes = 511;

    public void Append<T>(T indexable) where T : Indexable
    {
        if (indexable == null) throw new ArgumentNullException(nameof(indexable));

        using var tx = _env.BeginTransaction();
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });

        using var ms = new MemoryStream();
        indexable.WriteToStream(ms, UseGzip);

        var key = indexable.Id.ToByteArray();

        Index(db, tx, key, ms.ToArray());

        foreach (var property in typeof(T).GetProperties())
        {
            if (!property.HasAttribute<IndexedAttribute>())
                continue;
            if (!KeyBuilder.IndexKey(property, indexable, key, out var indexKey) || indexKey == null)
                continue;
            Index(db, tx, indexKey, key);
        }

        tx.Commit();
    }

    private static void Index(LightningDatabase db, LightningTransaction tx, byte[] key, byte[] value)
    {
        Debug.Assert(key.Length < MaxKeySizeBytes);
        if (key.Length > MaxKeySizeBytes)
            throw new InvalidOperationException($"Key length ({key.Length}) exceeds MaxKeySizeBytes ({MaxKeySizeBytes})");

        tx.Put(db, key, value, PutOptions.NoOverwrite);
    }

    public IEnumerable<T> Find<T>(string key, string? value, CancellationToken cancellationToken) where T : IIndexable, new()
    {
        var lookupKey = string.IsNullOrWhiteSpace(value)
            ? KeyBuilder.KeyPrefix(typeof(T), key)
            : KeyBuilder.LookupKey(typeof(T), key, value);

        return FindByKey<T>(lookupKey, cancellationToken);
    }

    public IEnumerable<T> FindByKey<T>(byte[] key, CancellationToken cancellationToken) where T : IIndexable, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.None });
        using var cursor = tx.CreateCursor(db);

        var entries = new List<T>();

        var sr = cursor.SetRange(key);
        if (sr != MDBResultCode.Success)
            return entries;

        var (r, k, v) = cursor.GetCurrent();

        while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
        {
            var next = k.AsSpan();
            if (!next.StartsWith(key))
                break;

            var index = v.AsSpan();
            var entry = GetById<T>(index, cancellationToken);
            if (entry == null)
                break;

            entries.Add(entry);

            r = cursor.Next();
            if (r == MDBResultCode.Success)
                (r, k, v) = cursor.GetCurrent();
        }

        return entries;
    }

    public T? GetById<T>(Guid id, CancellationToken cancellationToken) where T : IIndexable, new() => GetById<T>(id.ToByteArray(), cancellationToken);

    private T? GetById<T>(ReadOnlySpan<byte> id, CancellationToken cancellationToken) where T : IIndexable, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var tx = _env.BeginTransaction(TransactionBeginFlags.None);
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.None });
        using var cursor = tx.CreateCursor(db);

        var (sr, _, _) = cursor.SetKey(id);
        if (sr != MDBResultCode.Success)
            return default;

        var (gr, _, value) = cursor.GetCurrent();
        if (gr != MDBResultCode.Success)
            return default;

        var buffer = value.AsSpan().ToArray();
        using var ms = new MemoryStream(buffer);
        
        var deserialized = new T();
        deserialized.ReadFromStream(ms, UseGzip);
        return deserialized;
    }

    public ulong GetEntriesCount(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.None });
        var count = tx.GetEntriesCount(db);
        return (ulong) count;
    }

    public void Dispose()
    {
        _env.Dispose();
    }
}