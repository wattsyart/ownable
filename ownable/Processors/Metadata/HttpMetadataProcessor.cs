using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ownable.Models;

namespace ownable.Processors.Metadata;

public class HttpMetadataProcessor : IMetadataProcessor
{
    private readonly HttpClient _http;
    private readonly ILogger<IMetadataProcessor> _logger;

    public HttpMetadataProcessor(HttpClient http, ILogger<IMetadataProcessor> logger)
    {
        _http = http;
        _logger = logger;
    }

    public virtual bool CanProcess(string tokenUri)
    {
        return tokenUri.IsValidHttpUri();
    }

    public async Task<JsonTokenMetadata?> ProcessAsync(string tokenUri, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching metadata from {Uri}", tokenUri);
            await using var stream = await _http.GetStreamAsync(tokenUri, cancellationToken);
            var metadata = JsonSerializer.Deserialize<JsonTokenMetadata>(stream);
            return metadata;
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.TooManyRequests)
                _logger.LogError(e, "Throttled when fetching {Uri}", tokenUri);

            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to process metadata from {Uri}", tokenUri);
            return null;
        }
    }
}