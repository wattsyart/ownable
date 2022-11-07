using System.Text.Json.Serialization;
using ownable.Serialization;

namespace ownable.Models;

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
    [JsonConverter(typeof(EmptyStringAttributesConverter))]
    public List<JsonTokenMetadataAttribute> Attributes { get; set; } = new();
}