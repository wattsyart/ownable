using ownable;
using ownable.host.Extensions;
using ownable.host.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvcCore().AddJsonOptions(x => { x.JsonSerializerOptions.MapFrom(ServiceCollectionExtensions.GetJsonSerializerOptions()); });
builder.Services.AddHttpClient();
builder.Services.AddServerIndexingServices(builder.Configuration);

IConfiguration configuration = builder.Configuration;

CommandLine.AddCommand("serve", (_, _, _) => Serve());
CommandLine.ProcessArguments(ref configuration, builder.Services, args);

IConfiguration Serve()
{
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHostedService<SyncService>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseWebAssemblyDebugging();
    }

    app.UseHttpsRedirection();
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapFallbackToFile("index.html");
    app.Run();

    return builder.Configuration;
}