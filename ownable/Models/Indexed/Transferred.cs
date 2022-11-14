using System.Numerics;
using ownable.Serialization;

namespace ownable.Models.Indexed;

public abstract class Transferred : Indexable
{
    [Indexed] public string? Address { get; set; }
    [Indexed] public string ContractAddress { get; set; } = null!;
    [Indexed] public BigInteger TokenId { get; set; }

    public override void Serialize(IndexSerializeContext context)
    {
        base.Serialize(context);
        context.bw.WriteNullableString(Address);
        context.bw.Write(ContractAddress);
        context.bw.Write(TokenId);
    }

    public override void Deserialize(IndexDeserializeContext context)
    {
        base.Deserialize(context);
        Address = context.br.ReadNullableString();
        ContractAddress = context.br.ReadString();
        TokenId = context.br.ReadBigInteger();
    }
}