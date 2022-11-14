using System.Numerics;
using ownable.Serialization;

namespace ownable.Models.Indexed;

public class Trait : Indexable
{
    [Indexed] public string? Type { get; set; }
    [Indexed] public object? Value { get; set; }
    [Indexed] public string? ContractAddress { get; set; }
    [Indexed] public BigInteger TokenId { get; set; }

    public override void Serialize(IndexSerializeContext context)
    {
        base.Serialize(context);
        context.bw.WriteNullableString(Type);
        context.bw.WriteNullableString(Value?.ToString());
        context.bw.WriteNullableString(ContractAddress);
        context.bw.Write(TokenId);
    }

    public override void Deserialize(IndexDeserializeContext context)
    {
        base.Deserialize(context);
        Type = context.br.ReadNullableString();
        Value = context.br.ReadNullableString();
        ContractAddress = context.br.ReadNullableString();
        TokenId = context.br.ReadBigInteger();
    }
}