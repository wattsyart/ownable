
using ownable.Serialization;
using System.IO.Compression;

namespace ownable.Models.Indexed;

public sealed class Contract : IIndexed
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Indexed]
    public string? Address { get; set; }

    [Indexed]
    public string? BlockNumber { get; set; }

    [Indexed]
    public string? Type { get; set; }

    [Indexed]
    public string? Name { get; set; }

    [Indexed]
    public string? Symbol { get; set; }

    public void Serialize(IndexSerializeContext context)
    {
        context.bw.Write(Id);
        context.bw.WriteNullableString(Address);
        context.bw.WriteNullableString(BlockNumber);
        context.bw.WriteNullableString(Type);
        context.bw.WriteNullableString(Name);
        context.bw.WriteNullableString(Symbol);
    }

    public void Deserialize(IndexDeserializeContext context)
    {
        Id = context.br.ReadGuid();
        Address = context.br.ReadNullableString();
        BlockNumber = context.br.ReadNullableString();
        Type = context.br.ReadNullableString();
        Name = context.br.ReadNullableString();
        Symbol = context.br.ReadNullableString();
    }

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