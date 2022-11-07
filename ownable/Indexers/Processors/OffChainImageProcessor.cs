using ownable.Models;

namespace ownable.Indexers.Processors;

internal sealed class OffChainImageProcessor : IMetadataImageProcessor
{
    private readonly HttpClient _http;

    public OffChainImageProcessor(HttpClient http)
    {
        _http = http;
    }

    public bool CanProcess(JsonTokenMetadata metadata)
    {
        return Uri.TryCreate(metadata.Image, UriKind.Absolute, out _) ||
               Uri.TryCreate(metadata.ImageData, UriKind.Absolute, out _);
    }

    public async Task<(Stream? stream, string? extension)> ProcessAsync(JsonTokenMetadata metadata, CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(metadata.Image, UriKind.Absolute, out var imageUri))
        {
            return (await _http.GetStreamAsync(imageUri, cancellationToken), Path.GetExtension(metadata.Image));
        }

        if (Uri.TryCreate(metadata.ImageData, UriKind.Absolute, out var imageDataUri))
        {
            return (await _http.GetStreamAsync(imageDataUri, cancellationToken), Path.GetExtension(metadata.ImageData));
        }

        return (null, null);
    }
}