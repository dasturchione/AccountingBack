using Application;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Infrastructure.Persistence;
using Infrastructure.Query;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;

namespace IntegrationTests.Infrastructure;

public static class ApplicationQueryTestServiceProvider
{
    public static ServiceProvider Create(
        PostgreSqlIntegrationFixture fixture,
        IUserContext userContext,
        Action<IServiceCollection>? configureFeature = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(userContext);
        services.AddSingleton<IUserContext>(userContext);
        services.AddApplication();
        services.AddScoped(_ => fixture.CreateDbContext(userContext));
        services.AddScoped<IQueryBuilderResolver, QueryBuilderResolver>();
        services.AddScoped<IQueryBuilder, QueryBuilder>();
        services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
        services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
        configureFeature?.Invoke(services);

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });
    }
}
