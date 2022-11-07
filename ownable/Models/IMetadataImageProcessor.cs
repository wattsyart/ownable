namespace ownable.Models;

public interface IMetadataImageProcessor
{
    bool CanProcess(JsonTokenMetadata metadata);
    Task<(Stream? stream, string? extension)> ProcessAsync(JsonTokenMetadata metadata, CancellationToken cancellationToken);
}