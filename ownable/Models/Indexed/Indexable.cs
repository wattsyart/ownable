using System.IO.Compression;
using ownable.Serialization;

namespace ownable.Models.Indexed;

public abstract class Indexable : IIndexable
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    public abstract void Serialize(IndexSerializeContext context);
    public abstract void Deserialize(IndexDeserializeContext context);

    public void WriteToStream(Stream stream)
    {
        using var zip = new GZipStream(stream, CompressionMode.Compress, true);
        using var bw = new BinaryWriter(zip);
        Serialize(new IndexSerializeContext(bw));
    }

    public void ReadFromStream(Stream stream)
    {
        using var unzip = new GZipStream(stream, CompressionMode.Decompress, true);
        using var br = new BinaryReader(unzip);
        Deserialize(new IndexDeserializeContext(br));
    }
}