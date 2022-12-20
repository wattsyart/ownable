using LightningDB;
using Microsoft.Extensions.Logging;

namespace ownable.store
{
    public sealed class KeyValueStore : IKeyValueStore
    {
        private const ushort MaxKeySizeBytes = 511;

        private readonly LightningEnvironment _env;
        private readonly ILogger<KeyValueStore> _logger;

        public string Path { get; set; }

        // ReSharper disable once UnusedMember.Global (Reflection)
        public KeyValueStore(ILogger<KeyValueStore> logger) : this("store", logger) { }

        public KeyValueStore(string path, ILogger<KeyValueStore> logger)
        {
            _logger = logger;
            Path = path;

            _env = new LightningEnvironment(path, new EnvironmentConfiguration { MapSize = 10_485_760 });
            _env.MaxDatabases = 1;
            _env.Open();
        }

        public bool TryGet(ReadOnlySpan<byte> key, out ReadOnlySpan<byte> value)
        {
            try
            {
                using var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
                var db = tx.OpenDatabase();
                if (tx.TryGet(db, key, out var data))
                {
                    value = data;
                    return true;
                }
                value = default;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving value for key from the database.");
                value = default;
                return false;
            }
        }

        public bool TryPut(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
        {
            try
            {
                using var tx = _env.BeginTransaction();
                var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
                tx.Put(db, key, value, PutOptions.None);
                tx.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing key-value pair in the database.");
                return false;
            }
        }
    }
}
