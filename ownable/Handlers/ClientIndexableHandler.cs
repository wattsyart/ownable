using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ownable.Models;
using ownable.Models.Indexed;

namespace ownable.Handlers;

public sealed class ClientIndexableHandler : IIndexableHandler
{
    private readonly HttpClient _http;
    private readonly ILogger<ClientIndexableHandler> _logger;
    private readonly JsonSerializerOptions _options;

    public ClientIndexableHandler(HttpClient http, JsonSerializerOptions options, ILogger<ClientIndexableHandler> logger)
    {
        _http = http;
        _logger = logger;
        _options = options;
    }

    public async Task<bool> HandleBatchAsync(List<Received> batch, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(batch, _options);

        var request = new HttpRequestMessage(HttpMethod.Put, "indices");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> HandleBatchAsync(List<Sent> batch, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(batch, _options);

        var request = new HttpRequestMessage(HttpMethod.Put, "indices");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}