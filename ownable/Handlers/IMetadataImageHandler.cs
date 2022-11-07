using System.Numerics;

namespace ownable.Handlers;

public interface IMetadataImageHandler
{
    Task<bool> HandleAsync(Stream stream, string contractAddress, BigInteger tokenId, BigInteger blockNumber, string? extension,
        CancellationToken cancellationToken);
}