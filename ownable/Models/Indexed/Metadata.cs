using System.Numerics;
using ownable.Serialization;

namespace ownable.Models.Indexed;

public class Metadata : Indexable
{
    [Indexed] public string? Name { get; set; }
    [Indexed] public string? Description { get; set; }
    [Indexed] public string? ExternalUrl { get; set; }

    [Indexed] public string? ContractAddress { get; set; }
    [Indexed] public BigInteger TokenId { get; set; }

    public override void Serialize(IndexSerializeContext context)
    {
        base.Serialize(context);
        context.bw.WriteNullableString(Name);
        context.bw.WriteNullableString(Description);
        context.bw.WriteNullableString(ExternalUrl);
        context.bw.WriteNullableString(ContractAddress);
        context.bw.Write(TokenId);
    }

    public override void Deserialize(IndexDeserializeContext context)
    {
        base.Deserialize(context);
        Name = context.br.ReadNullableString();
        Description = context.br.ReadNullableString();
        ExternalUrl = context.br.ReadNullableString();
        ContractAddress = context.br.ReadNullableString();
        TokenId = context.br.ReadBigInteger();
    }
}