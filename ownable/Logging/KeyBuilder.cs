using ownable.Data;
using System.Text;

namespace ownable.Logging;

internal static class KeyBuilder
{
    private static readonly byte[] LogEntryPrefix = Encoding.UTF8.GetBytes("E:");
    private static readonly byte[] LogDataKeyPrefix = Encoding.UTF8.GetBytes("K:");
    private static readonly byte[] LogDataValuePrefix = Encoding.UTF8.GetBytes(":V:");

    public static byte[] BuildLogByEntryKey(byte[] id) => LogEntryPrefix.Concat(id);

    public static byte[] GetLogByDataKey(string key) => LogDataKeyPrefix
        .Concat(Encoding.UTF8.GetBytes(key)).Concat(LogDataValuePrefix)
        .Concat(Encoding.UTF8.GetBytes("?"))
    ;

    public static byte[] GetLogByDataKey(string key, string? value) => LogDataKeyPrefix
        .Concat(Encoding.UTF8.GetBytes(key)).Concat(LogDataValuePrefix)
        .Concat(Encoding.UTF8.GetBytes(value ?? "?"))
    ;

    public static byte[] BuildLogByDataKey(string key, string? value, byte[] id) => LogDataKeyPrefix
        .Concat(Encoding.UTF8.GetBytes(key)).Concat(LogDataValuePrefix)
        .Concat(Encoding.UTF8.GetBytes(value ?? "?"))
        .Concat(BuildLogByEntryKey(id))
    ;

    public static byte[] GetAllLogEntriesKey() => LogEntryPrefix;
}