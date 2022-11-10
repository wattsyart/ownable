using ownable;
using ownable.host.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvcCore().AddJsonOptions(x =>
{
    var options = ServiceCollectionExtensions.GetJsonSerializerOptions();
    
    x.JsonSerializerOptions.Converters.Clear();
    foreach(var converter in options.Converters)
        x.JsonSerializerOptions.Converters.Add(converter);

    x.JsonSerializerOptions.AllowTrailingCommas = options.AllowTrailingCommas;
    x.JsonSerializerOptions.DefaultBufferSize = options.DefaultBufferSize;
    x.JsonSerializerOptions.DefaultIgnoreCondition = options.DefaultIgnoreCondition;
    x.JsonSerializerOptions.DictionaryKeyPolicy = options.DictionaryKeyPolicy;
    x.JsonSerializerOptions.Encoder = options.Encoder;
    x.JsonSerializerOptions.IgnoreReadOnlyFields = options.IgnoreReadOnlyFields;
    x.JsonSerializerOptions.IgnoreReadOnlyProperties = options.IgnoreReadOnlyProperties;
    x.JsonSerializerOptions.IncludeFields = options.IncludeFields;
    x.JsonSerializerOptions.NumberHandling = options.NumberHandling;
    x.JsonSerializerOptions.PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive;
    x.JsonSerializerOptions.PropertyNamingPolicy = options.PropertyNamingPolicy;
    x.JsonSerializerOptions.ReadCommentHandling = options.ReadCommentHandling;
    x.JsonSerializerOptions.ReferenceHandler = options.ReferenceHandler;
    x.JsonSerializerOptions.UnknownTypeHandling = options.UnknownTypeHandling;
    x.JsonSerializerOptions.WriteIndented = options.WriteIndented;
    x.JsonSerializerOptions.MaxDepth = options.MaxDepth;
});

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