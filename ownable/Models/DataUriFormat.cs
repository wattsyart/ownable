namespace ownable.Models;

public class DataUriFormat
{
    public bool IsBase64 { get; set; }
    public string? Extension { get; set; }
    public byte[]? Data { get; set; }
    public string? ContentType { get; set; }
}