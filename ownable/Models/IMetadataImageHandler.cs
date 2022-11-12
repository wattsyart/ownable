using System.Numerics;

namespace ownable.Models;

public interface IMetadataImageHandler
{
    Task<bool> HandleAsync(Stream stream, string contractAddress, BigInteger tokenId, BigInteger blockNumber, string? extension, IndexScope scope,
        CancellationToken cancellationToken);
}