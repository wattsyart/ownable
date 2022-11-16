using Microsoft.Extensions.Logging;
using ownable.Models;
using ownable.Models.Indexed;

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

    public Task<(Stream? stream, Media? media)> ProcessAsync(JsonTokenMetadata metadata, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(metadata.Image) && DataUri.TryParseImage(metadata.Image, out var imageFormat) && imageFormat is {Data: { }})
        {
            _logger.LogInformation("Fetching embedded {ContentType} image", imageFormat.ContentType);
            
            var media = new Media
            {
                Path = metadata.Image,
                ContentType = imageFormat.ContentType,
                Processor = GetType().Name,
                Extension = imageFormat.Extension
            };

            var ms = new MemoryStream(imageFormat.Data);
            return Task.FromResult(((Stream?) ms, (Media?) media));
        }

        if (!string.IsNullOrWhiteSpace(metadata.ImageData) && DataUri.TryParseImage(metadata.ImageData, out var imageDataFormat) && imageDataFormat is { Data: { } })
        {
            _logger.LogInformation("Fetching embedded {ContentType} image", imageDataFormat.ContentType);

            var media = new Media
            {
                Path = metadata.ImageData,
                ContentType = imageDataFormat.ContentType,
                Processor = GetType().Name,
                Extension = imageDataFormat.Extension
            };

            var ms = new MemoryStream(imageDataFormat.Data);
            return Task.FromResult(((Stream?)ms, (Media?)media));
        }

        return Task.FromResult(((Stream?) null, (Media?) null));
    }
}