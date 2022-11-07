using System.Numerics;

namespace ownable.Indexers.Handlers;

public interface IMetadataImageHandler
{
    Task<bool> HandleAsync(Stream stream, string contractAddress, BigInteger tokenId, string? extension,
        CancellationToken cancellationToken);
}