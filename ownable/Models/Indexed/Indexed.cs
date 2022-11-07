using System.IO.Compression;
using ownable.Serialization;

namespace ownable.Models.Indexed;

public abstract class Indexed : IIndexed
{
    public abstract void Serialize(IndexSerializeContext context);
    public abstract void Deserialize(IndexDeserializeContext context);

    public void WriteToFile(string path)
    {
        using var stream = File.Create(path);
        using var zip = new GZipStream(stream, CompressionMode.Compress, true);
        using var bw = new BinaryWriter(zip);

        Serialize(new IndexSerializeContext(bw));
    }

    public void ReadFromFile(string path)
    {
        using var stream = File.OpenRead(path);
        using var unzip = new GZipStream(stream, CompressionMode.Decompress, true);
        using var br = new BinaryReader(unzip);

        Deserialize(new IndexDeserializeContext(br));
    }
}