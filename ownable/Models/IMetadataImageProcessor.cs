using ownable.Models.Indexed;

namespace ownable.Models;

public interface IMetadataImageProcessor
{
    bool CanProcess(JsonTokenMetadata metadata);
    Task<(Stream? stream, Media? media)> ProcessAsync(JsonTokenMetadata metadata, CancellationToken cancellationToken);
}