using System.Diagnostics;
using LightningDB;
using ownable.Models;
using ownable.Models.Indexed;
using ownable.Serialization;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ownable.Data;

public class Store : IDisposable
{
    private readonly ILogger<Store> _logger;
    private readonly LightningEnvironment _env;
    
    public bool UseGzip { get; set; }

    // ReSharper disable once UnusedMember.Global (Reflection)
    public Store(ILogger<Store> logger) : this("store", logger) { }

    public Store(string path, ILogger<Store> logger)
    {
        _logger = logger;
        Path = path;

        _env = new LightningEnvironment(path, new EnvironmentConfiguration { MapSize = 10_485_760 });
        _env.MaxDatabases = 1;
        _env.Open();
    }

    public string Path { get; set; }
    
    private const ushort MaxKeySizeBytes = 511;

    private readonly Dictionary<Type, PropertyInfo[]> _cachedProperties = new();

    public void Append<T>(T indexable, CancellationToken cancellationToken) where T : IIndexable
    {
        if (indexable == null) throw new ArgumentNullException(nameof(indexable));

        using var tx = _env.BeginTransaction();
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });

        using var ms = new MemoryStream();
        indexable.WriteToStream(ms, UseGzip);

        var type = typeof(T);

        // GUID
        var key = indexable.Id.ToByteArray();

        // GUID => Buffer
        Index(db, tx, key, ms.ToArray());

        // Type => GUID
        Index(db, tx, KeyBuilder.IndexKey(type, nameof(Indexable.Id), indexable.Id.ToString(), key), key);

        if (!_cachedProperties.TryGetValue(type, out var properties))
            _cachedProperties.Add(type, properties = 
                type.GetProperties()
                .Where(x => x.CanRead && x.HasAttribute<IndexedAttribute>())
                .ToArray()
                );

        foreach (var property in properties)
            Index(db, tx, KeyBuilder.IndexKey(property, indexable, key), key);

        _logger.LogInformation("Before Append: {Count} entries", GetEntriesCount(cancellationToken));
        tx.Commit();
        _logger.LogInformation("After Append: {Count} entries", GetEntriesCount(cancellationToken));
    }

    private static void Index(LightningDatabase db, LightningTransaction tx, ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
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

    public IEnumerable<T> FindByKey<T>(ReadOnlySpan<byte> key, CancellationToken cancellationToken) where T : IIndexable, new()
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

    public IEnumerable<T> Get<T>(CancellationToken cancellationToken) where T : IIndexable, new()
    {
        return FindByKey<T>(KeyBuilder.KeyPrefix(typeof(T), nameof(Indexable.Id)), cancellationToken);
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