using System.IO.Compression;
using System.Numerics;
using ownable.Serialization;

namespace ownable.Models.Indexed;

public abstract class Indexable : IIndexable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Indexed] public BigInteger BlockNumber { get; set; }

    public virtual void Serialize(IndexSerializeContext context)
    {
        context.bw.Write(Id);
        context.bw.Write(BlockNumber);
    }

    public virtual void Deserialize(IndexDeserializeContext context)
    {
        Id = context.br.ReadGuid();
        BlockNumber = context.br.ReadBigInteger();
    }

    public void WriteToStream(Stream stream, bool gzip = false)
    {
        if(gzip)
        {
            using var zip = new GZipStream(stream, CompressionMode.Compress, true);
            using var bw = new BinaryWriter(zip);
            Serialize(new IndexSerializeContext(bw));
        }
        else
        {
            using var bw = new BinaryWriter(stream);
            Serialize(new IndexSerializeContext(bw));
        }       
    }

    public void ReadFromStream(Stream stream, bool gzipped = false)
    {
        if(gzipped)
        {
            using var unzip = new GZipStream(stream, CompressionMode.Decompress, true);
            using var br = new BinaryReader(unzip);
            Deserialize(new IndexDeserializeContext(br));
        }
        else
        {
            using var br = new BinaryReader(stream);
            Deserialize(new IndexDeserializeContext(br));
        }        
    }
}