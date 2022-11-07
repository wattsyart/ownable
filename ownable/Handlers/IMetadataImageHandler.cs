using System.Numerics;

namespace ownable.Handlers;

public interface IMetadataImageHandler
{
    Task<bool> HandleAsync(Stream stream, string contractAddress, BigInteger tokenId, string? extension,
        CancellationToken cancellationToken);
}