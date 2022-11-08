using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ownable.Serialization.Converters;

/// <summary>
/// Necessary for sensible serialization of BigInteger used by block numbers, etc., without reducing fidelity.
/// For example, ENS hashes routinely expand beyond UInt164. 
/// </summary>
public class BigIntegerConverter : JsonConverter<BigInteger>
{
    public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => BigInteger.Parse(JsonDocument.ParseValue(ref reader).RootElement.GetRawText(), NumberFormatInfo.InvariantInfo);
    public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options) => writer.WriteRawValue(value.ToString(NumberFormatInfo.InvariantInfo));
}