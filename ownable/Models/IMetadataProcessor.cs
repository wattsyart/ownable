namespace ownable.Models;

public interface IMetadataProcessor
{
    bool CanProcess(string tokenUri);
    Task<JsonTokenMetadata?> ProcessAsync(string tokenUri, CancellationToken cancellationToken);
}