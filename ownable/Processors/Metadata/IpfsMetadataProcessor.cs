using System.Text.Json;
using Ipfs.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ownable.Models;

namespace ownable.Processors.Metadata;

public class IpfsMetadataProcessor : IMetadataProcessor
{
    private readonly IOptionsMonitor<IpfsOptions> _options;
    private readonly ILogger<IMetadataProcessor> _logger;

    public IpfsMetadataProcessor(IOptionsMonitor<IpfsOptions> options, ILogger<IMetadataProcessor> logger)
    {
        _options = options;
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
        _logger.LogInformation("Fetching IPFS CID {CID} from gateway {Gateway}", cid, _options.CurrentValue.Gateway);

        var ipfs = new IpfsClient(_options.CurrentValue.Gateway);
        var json = await ipfs.FileSystem.ReadAllTextAsync(cid, cancellationToken);
        var metadata = JsonSerializer.Deserialize<JsonTokenMetadata?>(json);
        return metadata;
    }
}