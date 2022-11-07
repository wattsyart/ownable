using ownable.Models.Indexed;

namespace ownable.Serialization;

public sealed class RoundTrip
{
    public static void Check<TRoundTrip>(TRoundTrip toSerialize) where TRoundTrip : ISerialize<IndexSerializeContext>, IDeserialize<IndexDeserializeContext>, new() => Check<TRoundTrip, IndexSerializeContext, IndexDeserializeContext>(toSerialize);

    public static void Check<TRoundTrip, TSerializeContext, TDeserializeContext>(TRoundTrip toSerialize)
        where TRoundTrip : ISerialize<TSerializeContext>, IDeserialize<TDeserializeContext>, new()
        where TSerializeContext : ISerializeContext
        where TDeserializeContext : IDeserializeContext
    {
        var firstMemoryStream = new MemoryStream();
        var firstBinaryWriter = new BinaryWriter(firstMemoryStream);
        var firstSerializeContext = (TSerializeContext)Activator.CreateInstance(typeof(TSerializeContext), firstBinaryWriter)!;

        toSerialize.Serialize(firstSerializeContext);
        var originalData = firstMemoryStream.ToArray();

        var br = new BinaryReader(new MemoryStream(originalData));
        var deserializeContext = (TDeserializeContext)Activator.CreateInstance(typeof(TDeserializeContext), br)!;
        var deserialized = new TRoundTrip();
        deserialized.Deserialize(deserializeContext);

        var secondMemoryStream = new MemoryCompareStream(originalData);
        var secondBinaryWriter = new BinaryWriter(secondMemoryStream);
        var secondSerializeContext = (TSerializeContext)Activator.CreateInstance(typeof(TSerializeContext), secondBinaryWriter)!;
        deserialized.Serialize(secondSerializeContext);
    }
}