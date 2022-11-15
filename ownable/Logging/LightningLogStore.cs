using System.Diagnostics;
using LightningDB;
using Microsoft.Extensions.Logging;
using ownable.Models;
using ownable.Serialization;

namespace ownable.Logging;

internal sealed class LightningLogStore : ILogStore, IIndex
{
    private const ushort MaxKeySizeBytes = 511;
    private readonly LightningEnvironment _env;

    public string Path { get; set; }

    public ulong MapSize => (ulong)_env.MapSize;

    public LightningLogStore(string path)
    {
        Path = path;

        _env = new LightningEnvironment(path, new EnvironmentConfiguration { MapSize = 10_485_760 });
        _env.MaxDatabases = 1;
        _env.Open();
    }
    
    public bool Append<TState>(LogLevel logLevel, in EventId eventId, TState state, Exception? exception,
        Func<TState, Exception, string> formatter, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var id = Guid.NewGuid();
        var timestamp = DateTimeOffset.Now;

        var entry = new LogEntry(id, exception)
        {
            LogLevel = logLevel,
            EventId = eventId,
            Message = formatter(state, exception!),
            Timestamp = timestamp
        };

        if (state is IReadOnlyList<KeyValuePair<string, object?>> values)
        {
            entry.Data = values.Select(x => new KeyValuePair<string, string?>(x.Key, x.Value?.ToString())).ToList();
        }

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        var context = new LoggingSerializeContext(bw);
        entry.Serialize(context);

        var buffer = ms.ToArray();

        using var tx = _env.BeginTransaction();
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });

        cancellationToken.ThrowIfCancellationRequested();

        var key = id.ToByteArray();

        Index(db, tx, key, buffer);
        Index(db, tx, KeyBuilder.BuildLogByEntryKey(key), key);

        Index(db, tx, KeyBuilder.BuildLogByDataKey("ID", entry.Id.ToString(), key), key);
        Index(db, tx, KeyBuilder.BuildLogByDataKey("LOGLEVEL", GetLogLevelString(entry.LogLevel), key), key);
        Index(db, tx, KeyBuilder.BuildLogByDataKey("EVENTID.ID", entry.EventId.Id.ToString(), key), key);
        Index(db, tx, KeyBuilder.GetLogByDataKey("EVENTID.NAME", entry.EventId.Name ?? "?"), key);

        if (entry.Error != null)
        {
            Index(db, tx, KeyBuilder.BuildLogByDataKey("EXCEPTION.MESSAGE", entry.Error.Message ?? "?", key), key);
            Index(db, tx, KeyBuilder.BuildLogByDataKey("EXCEPTION.HELPLINK", entry.Error.HelpLink ?? "?", key), key);
            Index(db, tx, KeyBuilder.BuildLogByDataKey("EXCEPTION.SOURCE", entry.Error.Source ?? "?", key), key);
        }

        if (entry.Data != null)
        {
            foreach (var (k, v) in entry.Data)
            {
                var dataKey = KeyBuilder.BuildLogByDataKey(k.ToUpperInvariant(), v?.ToUpperInvariant(), key);
                if (dataKey.Length > MaxKeySizeBytes)
                {
                    Trace.TraceWarning($"Index Log Data Key '{k}' has length {dataKey.Length}, exceeding maximum of {MaxKeySizeBytes}");
                    continue;
                }

                Index(db, tx, dataKey, dataKey);
            }
        }

        return tx.Commit() == MDBResultCode.Success;
    }
    
    private static void Index(LightningDatabase db, LightningTransaction tx, ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        Debug.Assert(key.Length < MaxKeySizeBytes);
        if (key.Length > MaxKeySizeBytes)
            throw new InvalidOperationException($"Key length ({key.Length}) exceeds MaxKeySizeBytes ({MaxKeySizeBytes})");

        tx.Put(db, key, value, PutOptions.NoOverwrite);
    }

    private static string GetLogLevelString(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFORMATION",
            LogLevel.Warning => "WARNING",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRITICAL",
            LogLevel.None => "NONE",
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
    }

    public IEnumerable<LogEntry> Get(CancellationToken cancellationToken = default)
    {
        return GetByKey(KeyBuilder.GetAllLogEntriesKey(), cancellationToken);
    }

    public IEnumerable<LogEntry> GetByKey(string key, CancellationToken cancellationToken = default)
    {
        return GetByKey(KeyBuilder.GetLogByDataKey(key), cancellationToken);
    }

    public IEnumerable<LogEntry> GetByKeyAndValue(string key, string? value, CancellationToken cancellationToken = default)
    {
        return GetByKey(KeyBuilder.GetLogByDataKey(key, value), cancellationToken);
    }

    private IEnumerable<LogEntry> GetByKey(byte[] key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.None });
        using var cursor = tx.CreateCursor(db);

        var entries = new List<LogEntry>();

        var sr = cursor.SetRange(key);
        if (sr != MDBResultCode.Success)
            return entries;

        var (r, k, v) = cursor.GetCurrent();

        while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
        {
            if (!k.AsSpan().StartsWith(key))
                break;

            var index = v.AsSpan();
            var entry = GetByIndex(index, tx, cancellationToken);
            if (entry == default)
                break;

            entries.Add(entry);

            r = cursor.Next();
            if (r == MDBResultCode.Success)
                (r, k, v) = cursor.GetCurrent();
        }

        return entries;
    }

    private unsafe LogEntry? GetByIndex(ReadOnlySpan<byte> index, LightningTransaction? parent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var tx =
            _env.BeginTransaction(parent == null
                ? TransactionBeginFlags.ReadOnly
                : TransactionBeginFlags.None);

        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.None });
        using var cursor = tx.CreateCursor(db);

        var (sr, _, _) = cursor.SetKey(index);
        if (sr != MDBResultCode.Success)
            return default;

        var (gr, _, value) = cursor.GetCurrent();
        if (gr != MDBResultCode.Success)
            return default;

        var buffer = value.AsSpan();

        fixed (byte* buf = &buffer.GetPinnableReference())
        {
            using var ms = new UnmanagedMemoryStream(buf, buffer.Length);
            using var br = new BinaryReader(ms);
            var context = new LoggingDeserializeContext(br);

            var uuid = br.ReadGuid();
            var entry = new LogEntry(uuid, context);
            return entry;
        }
    }

    public ulong GetMapSizeInUse()
    {
        var stat = _env.EnvironmentStats;
        return (ulong)(stat.PageSize * (stat.LeafPages + stat.BranchPages + stat.OverflowPages));
    }

    public ulong GetEntriesCount()
    {
        using var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.None });
        var count = tx.GetEntriesCount(db);
        return (ulong)count;
    }

    public IndexInfo GetInfo()
    {
        return new IndexInfo
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(Path),
            EntriesCount = GetEntriesCount(),
            UsedSize = GetMapSizeInUse(),
            TotalSize = MapSize
        };
    }
}