using System.Text.Json;
using ownable.Models;

namespace ownable.Indexers.Metadata;

public class ArweaveMetadataProcessor : IMetadataProcessor
{
    private readonly HttpClient _http;

    public ArweaveMetadataProcessor(HttpClient http)
    {
        _http = http;
    }

    public bool CanProcess(string tokenUri)
    {
        return Uri.TryCreate(tokenUri, UriKind.Absolute, out var uri) &&
               uri.Host.Equals("arweave.net", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<JsonTokenMetadata?> ProcessAsync(string tokenUri, CancellationToken cancellationToken)
    {
        await using var stream = await _http.GetStreamAsync(tokenUri, cancellationToken);
        var metadata = JsonSerializer.Deserialize<JsonTokenMetadata>(stream);
        return metadata;
    }
}