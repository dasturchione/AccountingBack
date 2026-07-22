using WebApi.Configuration;

var builder = WebApplication.CreateBuilder();

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: false);

await builder.ConfigureAsync();

var app = builder.Build();

await app.ConfigureAsync();

app.Run();

public partial class Program;
