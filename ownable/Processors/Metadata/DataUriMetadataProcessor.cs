using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ownable.Models;

namespace ownable.Processors.Metadata;

public class DataUriMetadataProcessor : IMetadataProcessor
{
    private readonly ILogger<IMetadataProcessor> _logger;

    public DataUriMetadataProcessor(ILogger<IMetadataProcessor> logger)
    {
        _logger = logger;
    }

    public bool CanProcess(string tokenUri)
    {
        return DataUri.TryParseApplication(tokenUri, out _);
    }

    public Task<JsonTokenMetadata?> ProcessAsync(string tokenUri, CancellationToken cancellationToken)
    {
        if (!DataUri.TryParseApplication(tokenUri, out var format) || format is not { Data: { } })
            return Task.FromResult((JsonTokenMetadata?)null);

        try
        {
            _logger.LogInformation("Fetching embedded {ContentType} metadata", format.ContentType);
            var json = Encoding.UTF8.GetString(format.Data);
            var metadata = JsonSerializer.Deserialize<JsonTokenMetadata>(json);
            return Task.FromResult(metadata);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to process embedded {ContentType} metadata", format.ContentType);
            return Task.FromResult((JsonTokenMetadata?)null);
        }
    }
}