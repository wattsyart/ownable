using System.ComponentModel.DataAnnotations;

namespace ownable.Configuration;

public class Web3Options
{
    [Required]
    public string? RpcUrl { get; set; }
}