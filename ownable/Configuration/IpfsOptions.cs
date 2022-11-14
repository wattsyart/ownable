using System.ComponentModel.DataAnnotations;

namespace ownable.Configuration;

public class IpfsOptions
{
    [Required]
    public string? Gateway { get; set; }
    public string? Endpoint { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}