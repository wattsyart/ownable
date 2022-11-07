namespace ownable.Serialization;

public class IndexSerializeContext : ISerializeContext, IDisposable
{
    public IndexSerializeContext(BinaryWriter bw) : this(bw, FormatVersion) { }

    public IndexSerializeContext(BinaryWriter bw, int version)
    {
        Version = version;
        this.bw = bw;
        if (Version > FormatVersion)
            throw new Exception("Tried to save index with a version that is too new");
        bw.Write(Version);
    }

    // ReSharper disable once InconsistentNaming
    public readonly BinaryWriter bw;

    #region Version

    public const int FormatVersion = 1;
    public int Version { get; }

    #endregion

    public void Dispose()
    {
        bw.Dispose();
    }
}