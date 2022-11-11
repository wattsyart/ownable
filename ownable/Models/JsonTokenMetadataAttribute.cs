using System.Diagnostics;
using System.Text.Json.Serialization;

namespace ownable.Models;

[DebuggerDisplay("{TraitType} = {Value}")]
public sealed class JsonTokenMetadataAttribute
{
    [JsonPropertyName("trait_type")]
    public string? TraitType { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}