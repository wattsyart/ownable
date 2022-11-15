using System.Numerics;
using ownable.Models;
using ownable.Models.Indexed;

namespace ownable.Handlers;

internal sealed class FileImageHandler : IMetadataImageHandler
{
    public async Task<bool> HandleAsync(Stream stream, Media media, string contractAddress, BigInteger tokenId,
        BigInteger blockNumber, IndexScope scope, CancellationToken cancellationToken)
    {
        var imagesDir = "images";
        var outputDir = Path.Combine(imagesDir, contractAddress);
        var outputBlockDir = Path.Combine(outputDir, blockNumber.ToString());

        Directory.CreateDirectory(imagesDir);
        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(outputBlockDir);

        var path = Path.Combine(outputBlockDir, tokenId + media.Extension);
        if (File.Exists(path))
            File.Delete(path);

        await using var fs = File.OpenWrite(path);
        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);
        await stream.CopyToAsync(fs, cancellationToken);
        await fs.FlushAsync(cancellationToken);

        return File.Exists(path);
    }
}