namespace ownable.Serialization;

public interface IDeserialize<in TContext> where TContext : IDeserializeContext
{
    void Deserialize(TContext context);
    void ReadFromStream(Stream stream, bool gzipped = false);
}