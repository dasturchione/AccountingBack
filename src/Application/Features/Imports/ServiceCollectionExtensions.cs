using Application.Abstractions.Import;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Features.Imports;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImportModule(this IServiceCollection services)
    {
        services.AddScoped<IExcelImporter, ExcelImporter>();
        return services;
    }
}
