using System.Numerics;
using LightningDB;
using System.Text;
using ownable.Models;
using ownable.Models.Indexed;

namespace ownable;

public class Store : IDisposable
{
    private readonly LightningEnvironment _env;
    private readonly TypeRegistry _types;
    private readonly Dictionary<Type, Func<string, object>> _stringToObject;

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

    public void Index<T>(T instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        using var tx = _env.BeginTransaction();
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });

        tx.Put(db, _types.GetKey(instance), _types.GetKeyValue(instance), PutOptions.NoOverwrite);

        foreach (var indexed in _types.GetIndexed(instance))
        {
            var (key, value) = indexed(instance);
            var rawKey = Encoding.UTF8.GetString(key);
            var rawValue = Encoding.UTF8.GetString(value);
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

        foreach(var entry in entries)
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

                    var keyRawValue = Encoding.UTF8.GetString(k.AsSpan());
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

    public void Dispose()
    {
        _env.Dispose();
    }
}