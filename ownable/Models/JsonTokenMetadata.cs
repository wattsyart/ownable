using System.Diagnostics;
using System.Text.Json.Serialization;
using ownable.Serialization.Converters;

namespace ownable.Models;

[DebuggerDisplay("{Name} ({Attributes?.Count} attributes)")]
public sealed class JsonTokenMetadata
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("external_url")]
    public string? ExternalUrl { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("image_data")]
    public string? ImageData { get; set; }

    [JsonPropertyName("attributes")]
    public List<JsonTokenMetadataAttribute>? Attributes { get; set; }
}