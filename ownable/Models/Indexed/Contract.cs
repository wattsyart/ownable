
using ownable.Serialization;

namespace ownable.Models.Indexed;

public sealed class Contract : Indexable
{
    [Indexed]
    public string? Address { get; set; }

    [Indexed]
    public string? Type { get; set; }

    [Indexed]
    public string? Name { get; set; }

    [Indexed]
    public string? Symbol { get; set; }

    public override void Serialize(IndexSerializeContext context)
    {
        base.Serialize(context);
        context.bw.WriteNullableString(Address);
        context.bw.WriteNullableString(Type);
        context.bw.WriteNullableString(Name);
        context.bw.WriteNullableString(Symbol);
    }

    public override void Deserialize(IndexDeserializeContext context)
    {
        base.Deserialize(context);
        Address = context.br.ReadNullableString();
        Type = context.br.ReadNullableString();
        Name = context.br.ReadNullableString();
        Symbol = context.br.ReadNullableString();
    }
}