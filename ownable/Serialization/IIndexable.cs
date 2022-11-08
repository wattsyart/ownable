using System.Numerics;

namespace ownable.Serialization;

public interface IIndexable : ISerialize<IndexSerializeContext>, IDeserialize<IndexDeserializeContext>
{
    Guid Id { get; }
    BigInteger BlockNumber { get; }
}