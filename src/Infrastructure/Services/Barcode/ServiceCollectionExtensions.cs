using Application.Abstractions.Barcode;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Barcode;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBarcodeGenerator(this IServiceCollection services)
    {
        // Holatsiz servis — Singleton mosroq.
        services.AddSingleton<IBarcodeGenerator, BarcodeGenerator>();
        return services;
    }
}
