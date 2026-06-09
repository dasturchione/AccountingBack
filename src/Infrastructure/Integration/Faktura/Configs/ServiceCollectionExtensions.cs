using Application.Abstractions.Integration;
using Integration.Faktura.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Faktura.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFaktura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FakturaAuthSettings>(
            configuration.GetSection("FakturaAuthSettings"));

        services.AddSingleton<IFakturaService, FakturaService>();

        return services;
    }
}
