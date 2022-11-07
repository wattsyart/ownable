using System.Text.Json;
using Ipfs.Http;
using Microsoft.Extensions.Logging;
using ownable.Models;

namespace ownable.Processors.Metadata;

public class IpfsMetadataProcessor : IMetadataProcessor
{
    private readonly ILogger<IMetadataProcessor> _logger;

    public IpfsMetadataProcessor(ILogger<IMetadataProcessor> logger)
    {
        _logger = logger;
    }

    public bool CanProcess(string tokenUri)
    {
        return Uri.TryCreate(tokenUri, UriKind.Absolute, out var uri) &&
               uri.Scheme.Equals("ipfs", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<JsonTokenMetadata?> ProcessAsync(string tokenUri, CancellationToken cancellationToken)
    {
        var cid = tokenUri["ipfs://".Length..];
        _logger.LogInformation("Fetching IPFS CID {CID}", cid);

        var ipfs = new IpfsClient("https://ipfs.io");
        var json = await ipfs.FileSystem.ReadAllTextAsync(cid, cancellationToken);
        var metadata = JsonSerializer.Deserialize<JsonTokenMetadata?>(json);
        return metadata;
    }
}