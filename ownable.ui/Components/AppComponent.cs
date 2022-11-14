using System.Net.Http.Headers;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ownable.ui.Components;

public abstract class AppComponent : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = null!;

    [Inject]
    public NavigationManager Nav { get; set; } = null!;

    [Inject]
    public IJSRuntime Js { get; set; } = null!;
    
    [Inject]
    public ILocalStorageService LocalStorage { get; set; } = null!;

    [Inject]
    public JsonSerializerOptions JsonOptions { get; set; } = null!;

    [Inject]
    public ILogger<WalletComponent>? Logger { get; set; }

    public async Task<T?> GetOneAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return default;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        Logger?.LogTrace(body);
        var result = JsonSerializer.Deserialize<T>(body, JsonOptions);
        return result;
    }

    public async Task<IEnumerable<T>> GetManyAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return Enumerable.Empty<T>();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        Logger?.LogTrace(body);
        var result = JsonSerializer.Deserialize<IEnumerable<T>>(body, JsonOptions);
        return result ?? Enumerable.Empty<T>();
    }

    protected async Task TryAddHttpAuthenticationAsync(string account)
    {
        Http.DefaultRequestHeaders.Authorization = null;

        if (await LocalStorage.ContainKeyAsync($"JWT:{account}"))
        {
            var token = await LocalStorage.GetItemAsStringAsync($"JWT:{account}");
            if (!string.IsNullOrWhiteSpace(token))
            {
                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    token.Replace("\"", ""));
            }
        }
    }
}