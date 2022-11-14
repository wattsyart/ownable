using BlazorTable;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ownable.ui;
using ownable.ui.Models;
using ownable;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddClientIndexingServices();
builder.Services.AddMetamaskIntegration();
builder.Services.AddBlazorTable();

await builder.Build().RunAsync();
