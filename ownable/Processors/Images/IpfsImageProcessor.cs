using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ownable.Models;
using System.Net.Http.Headers;
using System.Text;
using ownable.Configuration;

namespace ownable.Processors.Images;

internal sealed class IpfsImageProcessor : IMetadataImageProcessor
{
    private readonly HttpClient _http;
    private readonly IOptionsMonitor<IpfsOptions> _options;
    private readonly ILogger<IMetadataImageProcessor> _logger;

    public IpfsImageProcessor(HttpClient http, IOptionsMonitor<IpfsOptions> options, ILogger<IMetadataImageProcessor> logger)
    {
        _http = http;
        _options = options;
        _logger = logger;
    }

    public bool CanProcess(JsonTokenMetadata metadata)
    {
        return (Uri.TryCreate(metadata.Image, UriKind.Absolute, out var uri) ||
                Uri.TryCreate(metadata.ImageData, UriKind.Absolute, out uri)) &&
               uri.Scheme.Equals("ipfs", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<(Stream? stream, string? extension)> ProcessAsync(JsonTokenMetadata metadata, CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(metadata.Image, UriKind.Absolute, out var imageUri))
        {
            return await FetchFromIpfsAsync(cancellationToken, imageUri);
        }

        if (Uri.TryCreate(metadata.ImageData, UriKind.Absolute, out var imageDataUri))
        {
            return await FetchFromIpfsAsync(cancellationToken, imageDataUri);
        }

        return (null, null);
    }

    private async Task<(Stream? stream, string? extension)> FetchFromIpfsAsync(CancellationToken cancellationToken, Uri imageDataUri)
    {
        var cid = imageDataUri.OriginalString["ipfs://".Length..];
        var extension = await GetExtensionAsync(cid, cancellationToken);
        
        Stream? stream;
        if (!string.IsNullOrWhiteSpace(_options.CurrentValue.Endpoint))
        {
            stream = await GetMediaStreamFromEndpoint(cid, cancellationToken);
        }
        else
        {
            stream = await GetMediaStreamFromGateway(cid, cancellationToken);
        }

        return (stream, extension);
    }

    private async Task<Stream?> GetMediaStreamFromGateway(string cid, CancellationToken cancellationToken)
    {
        Stream? stream;
        _logger.LogInformation("Fetching IPFS CID {CID} from gateway {Gateway}", cid, _options.CurrentValue.Gateway);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.CurrentValue.Gateway}/ipfs/{cid}");
        if (!string.IsNullOrWhiteSpace(_options.CurrentValue.Username))
        {
            var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.CurrentValue.Username}:{_options.CurrentValue.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
        }

        var response = await _http.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        }
        else
        {
            stream = null;
        }

        return stream;
    }

    private async Task<Stream?> GetMediaStreamFromEndpoint(string cid, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching IPFS CID {CID} from endpoint {Endpoint}", cid, _options.CurrentValue.Endpoint);

        var requestUri = $"{_options.CurrentValue.Endpoint}/api/v0/cat?arg={cid}";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        if (!string.IsNullOrWhiteSpace(_options.CurrentValue.Username))
        {
            var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.CurrentValue.Username}:{_options.CurrentValue.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
        }

        var response = await _http.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        return null;
    }

    private async Task<string> GetExtensionAsync(string cid, CancellationToken cancellationToken)
    {
        var extension = ".zip";
        var request = new HttpRequestMessage(HttpMethod.Head, $"{_options.CurrentValue.Gateway}/ipfs/{cid}");
        if (!string.IsNullOrWhiteSpace(_options.CurrentValue.Username))
        {
            var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.CurrentValue.Username}:{_options.CurrentValue.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
        }
        var response = await _http.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!string.IsNullOrWhiteSpace(mediaType) && mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
               extension = $".{mediaType["image/".Length..]}";

            _logger.LogInformation("Determined IPFS CID {CID} media extension to be {Extension}", cid, extension);
        }
        else
        {
            _logger.LogWarning("Could not determine IPFS CID {CID} media extension, assuming {Extension} ({StatusCode})", cid, extension, response.StatusCode);
        }

        return extension;
    }
}