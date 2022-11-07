using System.Text;
using System.Text.Json;
using ownable.Models;

namespace ownable.Processors;

public class DataUriMetadataProcessor : IMetadataProcessor
{
    public bool CanProcess(string tokenUri)
    {
        return DataUri.TryParseApplication(tokenUri, out _);
    }

    public Task<JsonTokenMetadata?> ProcessAsync(string tokenUri, CancellationToken cancellationToken)
    {
        if (!DataUri.TryParseApplication(tokenUri, out var format) || format is not {Data: { }})
            return Task.FromResult((JsonTokenMetadata?) null);

        var json = Encoding.UTF8.GetString(format.Data);
        var metadata = JsonSerializer.Deserialize<JsonTokenMetadata>(json);
        return Task.FromResult(metadata);
    }
}