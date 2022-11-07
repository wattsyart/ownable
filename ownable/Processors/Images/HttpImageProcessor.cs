using Microsoft.Extensions.Logging;
using ownable.Models;

namespace ownable.Processors.Images;

internal sealed class HttpImageProcessor : IMetadataImageProcessor
{
    private readonly HttpClient _http;
    private readonly ILogger<IMetadataImageProcessor> _logger;

    public HttpImageProcessor(HttpClient http, ILogger<IMetadataImageProcessor> logger)
    {
        _http = http;
        _logger = logger;
    }

    public bool CanProcess(JsonTokenMetadata metadata)
    {
        return (Uri.TryCreate(metadata.Image, UriKind.Absolute, out var uri) ||
                Uri.TryCreate(metadata.ImageData, UriKind.Absolute, out uri)) &&
               (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ||
                uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<(Stream? stream, string? extension)> ProcessAsync(JsonTokenMetadata metadata, CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(metadata.Image, UriKind.Absolute, out var imageUri))
        {
            _logger.LogInformation("Fetching image from {Uri}", imageUri);
            return (await _http.GetStreamAsync(imageUri, cancellationToken), Path.GetExtension(metadata.Image));
        }

        if (Uri.TryCreate(metadata.ImageData, UriKind.Absolute, out var imageDataUri))
        {
            _logger.LogInformation("Fetching image from {Uri}", imageDataUri);
            return (await _http.GetStreamAsync(imageDataUri, cancellationToken), Path.GetExtension(metadata.ImageData));
        }

        return (null, null);
    }
}