using Microsoft.Extensions.Logging;
using ownable.Models;
using ownable.Models.Indexed;

namespace ownable.Processors.Images;

internal class HttpImageProcessor : IMetadataImageProcessor
{
    private readonly HttpClient _http;
    private readonly ILogger<IMetadataImageProcessor> _logger;

    public HttpImageProcessor(HttpClient http, ILogger<IMetadataImageProcessor> logger)
    {
        _http = http;
        _logger = logger;
    }

    public virtual bool CanProcess(JsonTokenMetadata metadata) => metadata.HasHttpImage();

    public async Task<(Stream? stream, Media? media)> ProcessAsync(JsonTokenMetadata metadata, CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(metadata.Image, UriKind.Absolute, out var imageUri))
        {
            _logger.LogInformation("Fetching image from {Uri}", imageUri);

            var media = new Media
            {
                Path = metadata.Image,
                Processor = GetType().Name,
                Extension = Path.GetExtension(metadata.Image)
            };

            var response = await _http.GetAsync(imageUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (null, null);

            media.ContentType = response.Content.Headers.ContentType?.MediaType;
            return (await response.Content.ReadAsStreamAsync(cancellationToken), media);
        }

        if (Uri.TryCreate(metadata.ImageData, UriKind.Absolute, out var imageDataUri))
        {
            _logger.LogInformation("Fetching image from {Uri}", imageDataUri);

            var media = new Media
            {
                Path = metadata.ImageData,
                Processor = GetType().Name,
                Extension = Path.GetExtension(metadata.Image)
            };

            var response = await _http.GetAsync(imageUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (null, null);

            media.ContentType = response.Content.Headers.ContentType?.MediaType;
            return (await response.Content.ReadAsStreamAsync(cancellationToken), media);
        }

        return (null, null);
    }
}