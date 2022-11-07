using ownable;
using ownable.host.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddCoreServices(builder.Configuration);

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
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();

    return builder.Configuration;
}