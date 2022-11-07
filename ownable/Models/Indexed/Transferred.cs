using ownable.Serialization;

namespace ownable.Models.Indexed;

public abstract class Transferred : Indexed
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Indexed]
    public string? Address { get; set; }

    [Indexed]
    public string? BlockNumber { get; set; }

    [Indexed]
    public string? ContractAddress { get; set; }

    [Indexed]
    public string? TokenId { get; set; }

    public override void Serialize(IndexSerializeContext context)
    {
        context.bw.Write(Id);
        context.bw.WriteNullableString(Address);
        context.bw.WriteNullableString(BlockNumber);
        context.bw.WriteNullableString(ContractAddress);
        context.bw.WriteNullableString(TokenId);
    }

    public override void Deserialize(IndexDeserializeContext context)
    {
        Id = context.br.ReadGuid();
        Address = context.br.ReadNullableString();
        BlockNumber = context.br.ReadNullableString();
        ContractAddress = context.br.ReadNullableString();
        TokenId = context.br.ReadNullableString();
    }
}