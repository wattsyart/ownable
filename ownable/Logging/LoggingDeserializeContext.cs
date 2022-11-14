namespace ownable.Logging;

public sealed class LoggingDeserializeContext
{
    // ReSharper disable once InconsistentNaming
    public readonly BinaryReader br;

    public LoggingDeserializeContext(BinaryReader br)
    {
        this.br = br;
        Version = br.ReadUInt64();
        if (Version > LoggingSerializeContext.FormatVersion)
            throw new Exception("Tried to load log entry with a version that is too new");
    }

    public ulong Version { get; }
}