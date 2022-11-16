using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ownable.Configuration;
using ownable.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ownable.Processors.Metadata;

public sealed class IpfsMetadataProcessor : IMetadataProcessor
{
    private readonly HttpClient _http;
    private readonly IOptionsMonitor<IpfsOptions> _options;
    private readonly ILogger<IMetadataProcessor> _logger;

    public IpfsMetadataProcessor(HttpClient http, IOptionsMonitor<IpfsOptions> options, ILogger<IMetadataProcessor> logger)
    {
        _http = http;
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

        JsonTokenMetadata? metadata;
        if (!string.IsNullOrWhiteSpace(_options.CurrentValue.Endpoint))
        {
            metadata = await GetMetadataFromEndpoint(cancellationToken, cid);
        }
        else
        {
            metadata = await GetMetadataFromGateway(cancellationToken, cid);
        }

        return metadata;
    }

    private async Task<JsonTokenMetadata?> GetMetadataFromGateway(CancellationToken cancellationToken, string cid)
    {
        JsonTokenMetadata? metadata;
        _logger.LogInformation("Fetching IPFS CID {CID} from gateway {Gateway}", cid, _options.CurrentValue.Gateway);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.CurrentValue.Gateway}/ipfs/{cid}");
        if (!string.IsNullOrWhiteSpace(_options.CurrentValue.Username))
        {
            var authHeaderValue =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_options.CurrentValue.Username}:{_options.CurrentValue.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
        }

        var response = await _http.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            metadata = JsonSerializer.Deserialize<JsonTokenMetadata>(body);
        }
        else
        {
            metadata = null;
        }

        return metadata;
    }

    private async Task<JsonTokenMetadata?> GetMetadataFromEndpoint(CancellationToken cancellationToken, string cid)
    {
        JsonTokenMetadata? metadata;
        _logger.LogInformation("Fetching IPFS CID {CID} from endpoint {Endpoint}", cid, _options.CurrentValue.Endpoint);

        var requestUri = $"{_options.CurrentValue.Endpoint}/api/v0/cat?arg={cid}";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        if (!string.IsNullOrWhiteSpace(_options.CurrentValue.Username))
        {
            var authHeaderValue =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_options.CurrentValue.Username}:{_options.CurrentValue.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
        }

        var response = await _http.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            metadata = JsonSerializer.Deserialize<JsonTokenMetadata>(body);
        }
        else
        {
            metadata = null;
        }

        return metadata;
    }
}