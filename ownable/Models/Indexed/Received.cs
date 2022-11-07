namespace ownable.Models.Indexed;

public sealed class Received
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Indexed]
    public string Address { get; set; } = null!;

    [Indexed]
    public string? BlockNumber { get; set; }

    [Indexed]
    public string ContractAddress { get; set; } = null!;

    [Indexed]
    public string? TokenId { get; set; }
}