using ownable.Models;

namespace ownable.Indexers.Metadata;

public interface IMetadataProcessor
{
    bool CanProcess(string tokenUri);
    Task<JsonTokenMetadata?> ProcessAsync(string tokenUri, CancellationToken cancellationToken);
}