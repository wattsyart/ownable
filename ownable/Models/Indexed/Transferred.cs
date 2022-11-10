using System.Numerics;
using Newtonsoft.Json;
using ownable.Serialization;
using ownable.Serialization.Converters;

namespace ownable.Models.Indexed;

public abstract class Transferred : Indexable
{
    [Indexed]
    public string? Address { get; set; }

    [Indexed]
    public string ContractAddress { get; set; } = null!;

    [Indexed]
    [JsonConverter(typeof(BigIntegerConverter))]
    public BigInteger TokenId { get; set; }

    public override void Serialize(IndexSerializeContext context)
    {
        context.bw.Write(Id);
        context.bw.WriteNullableString(Address);
        context.bw.Write(BlockNumber);
        context.bw.Write(ContractAddress);
        context.bw.Write(TokenId);
    }

    public override void Deserialize(IndexDeserializeContext context)
    {
        Id = context.br.ReadGuid();
        Address = context.br.ReadNullableString();
        BlockNumber = context.br.ReadBigInteger();
        ContractAddress = context.br.ReadString();
        TokenId = context.br.ReadBigInteger();
    }
}