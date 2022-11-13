using Nethereum.Hex.HexTypes;
using ownable.Serialization;

namespace ownable.Models;

public sealed class Query
{
    public HexBigInteger FilterId { get; set; } = null!;

    public void Serialize(BinaryWriter bw)
    {
        bw.Write(FilterId.Value);
    }

    public void Deserialize(BinaryReader br)
    {
        FilterId = new HexBigInteger(br.ReadBigInteger());
    }
}