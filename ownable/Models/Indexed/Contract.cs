
namespace ownable.Models.Indexed;

public sealed class Contract
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
}