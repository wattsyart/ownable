namespace ownable.Models
{
    public sealed class Contract
    {
        [Key]
        public string? Address { get; set; }

        [Indexed]
        public string? Type { get; set; }
    }
}
