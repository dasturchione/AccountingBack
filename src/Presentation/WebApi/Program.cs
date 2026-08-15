using WebApi.Configuration;

var builder = WebApplication.CreateBuilder();

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

await builder.ConfigureAsync();

var app = builder.Build();

await app.ConfigureAsync();

app.Run();

public partial class Program;
