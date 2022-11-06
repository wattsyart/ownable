using LightningDB;
using System.Text;
using ownable.Models;

namespace ownable
{
    public class Store
    {
        private readonly LightningEnvironment _env;
        private readonly TypeRegistry _types;

        public Store()
        {
            _env = new LightningEnvironment("store", new EnvironmentConfiguration { MapSize = 10_485_760 });
            _env.MaxDatabases = 1;
            _env.Open();

            _types = new TypeRegistry();
            _types.Register<Contract>();
        }

        public void Index<T>(T instance)
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
            using var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.None });
            using var cursor = tx.CreateCursor(db);

            var entries = new Dictionary<string, T>();

            {
                var keyPrefix = _types.GetKeyPrefix<Contract>();
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

                    var key = Encoding.UTF8.GetString(index);
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

                        _types.SetField(entry.Value, field, Encoding.UTF8.GetString(index));

                        r = cursor.Next();
                        if (r == MDBResultCode.Success)
                            (r, k, v) = cursor.GetCurrent();
                    }
                }
            }

            return entries.Values;
        }
    }
}
