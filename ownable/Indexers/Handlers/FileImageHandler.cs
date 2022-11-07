using System.Numerics;

namespace ownable.Indexers.Handlers;

internal sealed class FileImageHandler : IMetadataImageHandler
{
    public async Task<bool> HandleAsync(Stream stream, string contractAddress, BigInteger tokenId, string? extension, CancellationToken cancellationToken)
    {
        var imagesDir = "images";
        var outputDir = Path.Combine(imagesDir, contractAddress);

        Directory.CreateDirectory(imagesDir);
        Directory.CreateDirectory(outputDir);

        var path = Path.Combine(outputDir, tokenId + extension);
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