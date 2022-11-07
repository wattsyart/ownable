using ownable.Models;

namespace ownable.Indexers.Metadata;

public interface IMetadataIndexer
{
    Task Index(JsonTokenMetadata metadata);
}