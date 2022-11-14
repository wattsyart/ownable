namespace ownable.Logging;

public sealed class LoggingSerializeContext
{
    public const ulong FormatVersion = 1UL;

    // ReSharper disable once InconsistentNaming
    public readonly BinaryWriter bw;

    public LoggingSerializeContext(BinaryWriter bw, ulong version = FormatVersion)
    {
        this.bw = bw;
        if (Version > FormatVersion)
            throw new Exception("Tried to save log entry with a version that is too new");
        Version = version;
        bw.Write(Version);
    }

    public ulong Version { get; }
}