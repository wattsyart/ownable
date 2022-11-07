using Microsoft.Extensions.Logging;
using ownable.Models;

namespace ownable.Processors.Images;

internal sealed class DataUriImageProcessor : IMetadataImageProcessor
{
    private readonly ILogger<IMetadataImageProcessor> _logger;

    public DataUriImageProcessor(ILogger<IMetadataImageProcessor> logger)
    {
        _logger = logger;
    }

    public bool CanProcess(JsonTokenMetadata metadata)
    {
        return (!string.IsNullOrWhiteSpace(metadata.Image) && DataUri.TryParseImage(metadata.Image, out _)) ||
               (!string.IsNullOrWhiteSpace(metadata.ImageData) && DataUri.TryParseImage(metadata.ImageData, out _));
    }

    public Task<(Stream? stream, string? extension)> ProcessAsync(JsonTokenMetadata metadata, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(metadata.Image) && DataUri.TryParseImage(metadata.Image, out var imageFormat) && imageFormat is {Data: { }})
        {
            _logger.LogInformation("Fetching embedded {ContentType} image", imageFormat.ContentType);
            var ms = new MemoryStream(imageFormat.Data);
            return Task.FromResult(((Stream?) ms, imageFormat.Extension));
        }

        if (!string.IsNullOrWhiteSpace(metadata.ImageData) && DataUri.TryParseImage(metadata.ImageData, out var imageDataFormat) && imageDataFormat is { Data: { } })
        {
            _logger.LogInformation("Fetching embedded {ContentType} image", imageDataFormat.ContentType);
            var ms = new MemoryStream(imageDataFormat.Data);
            return Task.FromResult(((Stream?)ms, imageDataFormat.Extension));
        }

        return Task.FromResult(((Stream?) null, (string?) null));
    }
}