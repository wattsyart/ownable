using ownable.Models.Indexed;

namespace ownable.Serialization;

public interface IIndexed : ISerialize<IndexSerializeContext>, IDeserialize<IndexDeserializeContext> { }