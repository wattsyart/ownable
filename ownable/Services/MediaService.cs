using System.Numerics;

namespace ownable.Services;

public sealed class MediaService
{
    public async Task<string?> GetMediaAsync(string contractAddress, BigInteger tokenId, BigInteger blockNumber, string extension, CancellationToken cancellationToken)
    {
        var imagesDir = "images";
        var outputDir = Path.Combine(imagesDir, contractAddress);
        var outputBlockDir = Path.Combine(outputDir, blockNumber.ToString());

        Directory.CreateDirectory(imagesDir);
        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(outputBlockDir);

        var path = Path.Combine(outputBlockDir, tokenId + extension);
        if (File.Exists(path))
        {
            var buffer = await File.ReadAllBytesAsync(path, cancellationToken);
            return Convert.ToBase64String(buffer);
        }
        else
        {
            return null;
        }
    }
}