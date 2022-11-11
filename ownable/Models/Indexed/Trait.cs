using System.Numerics;
using System.Text.Json.Serialization;
using ownable.Serialization;
using ownable.Serialization.Converters;

namespace ownable.Models.Indexed;

public class Trait : Indexable
{
    public string? Type { get; set; }
    public object? Value { get; set; }

    [Indexed]
    [JsonConverter(typeof(BigIntegerConverter))]
    public BigInteger TokenId { get; set; }

    public override void Serialize(IndexSerializeContext context)
    {
        base.Serialize(context);
        context.bw.WriteNullableString(Type);
        context.bw.WriteNullableString(Value?.ToString());
        context.bw.Write(TokenId);
    }

    public override void Deserialize(IndexDeserializeContext context)
    {
        base.Deserialize(context);
        Type = context.br.ReadNullableString();
        Value = context.br.ReadNullableString();
        TokenId = context.br.ReadBigInteger();
    }
}