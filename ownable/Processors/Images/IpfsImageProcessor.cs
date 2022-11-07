using Ipfs.Http;
using Microsoft.Extensions.Logging;
using ownable.Models;

namespace ownable.Processors.Images;

internal sealed class IpfsImageProcessor : IMetadataImageProcessor
{
    private readonly HttpClient _http;
    private readonly ILogger<IMetadataImageProcessor> _logger;

    public IpfsImageProcessor(HttpClient http, ILogger<IMetadataImageProcessor> logger)
    {
        _http = http;
        _logger = logger;
    }

    public bool CanProcess(JsonTokenMetadata metadata)
    {
        return (Uri.TryCreate(metadata.Image, UriKind.Absolute, out var uri) ||
                Uri.TryCreate(metadata.ImageData, UriKind.Absolute, out uri)) &&
               uri.Scheme.Equals("ipfs", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<(Stream? stream, string? extension)> ProcessAsync(JsonTokenMetadata metadata, CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(metadata.Image, UriKind.Absolute, out var imageUri))
        {
            return await FetchFromIpfsAsync(cancellationToken, imageUri);
        }

        if (Uri.TryCreate(metadata.ImageData, UriKind.Absolute, out var imageDataUri))
        {
            return await FetchFromIpfsAsync(cancellationToken, imageDataUri);
        }

        return (null, null);
    }

    private async Task<(Stream? stream, string? extension)> FetchFromIpfsAsync(CancellationToken cancellationToken, Uri imageDataUri)
    {
        var cid = imageDataUri.OriginalString["ipfs://".Length..];
        var extension = await GetExtensionAsync(imageDataUri, cid, cancellationToken);

        var ipfs = new IpfsClient("https://ipfs.io");
        _logger.LogInformation("Fetching IPFS CID {CID}", cid);
        var stream = await ipfs.FileSystem.ReadFileAsync(cid, cancellationToken);
        return (stream, extension);
    }

    private async Task<string> GetExtensionAsync(Uri imageDataUri, string cid, CancellationToken cancellationToken)
    {
        var extension = ".zip";
        var request = new HttpRequestMessage(HttpMethod.Head, $"https://ipfs.io/ipfs/{cid}");
        var response = await _http.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!string.IsNullOrWhiteSpace(mediaType) && mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                extension = $".{mediaType["image/".Length..]}";
        }
        return extension;
    }
}