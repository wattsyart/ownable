using System.Numerics;
using ownable.Models.Indexed;

namespace ownable.Models;

public interface IMetadataImageHandler
{
    Task<bool> HandleAsync(Stream stream, Media media, string contractAddress, BigInteger tokenId,
        BigInteger blockNumber, IndexScope scope, CancellationToken cancellationToken);
}