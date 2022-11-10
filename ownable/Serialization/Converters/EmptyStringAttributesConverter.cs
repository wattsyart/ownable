using System.Text.Json;
using System.Text.Json.Serialization;
using ownable.Models;

namespace ownable.Serialization.Converters;

/// <summary>
/// Mitigation for when metadata returns an empty string instead of an empty array or null for attributes.
/// First encountered in contract: 0x816b108c0ed83528ab465c172a4b50b5e152cf22 (VectorField) 
/// </summary>
public sealed class EmptyStringAttributesConverter : JsonConverter<List<JsonTokenMetadataAttribute>>
{
    public override List<JsonTokenMetadataAttribute>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<JsonTokenMetadataAttribute>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, List<JsonTokenMetadataAttribute> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, options);
    }
}