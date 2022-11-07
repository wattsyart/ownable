namespace ownable.Serialization;

public sealed class IndexDeserializeContext : IDeserializeContext, IDisposable
{
    public int Version { get; }

    // ReSharper disable once InconsistentNaming
    public readonly BinaryReader br;

    public IndexDeserializeContext(BinaryReader br)
    {
        this.br = br;
        Version = br.ReadInt32();
        if (Version > IndexSerializeContext.FormatVersion)
            throw new Exception("Tried to load index with a version that is too new");
    }

    public void Dispose()
    {
        br.Dispose();
    }
}