using Application.Abstractions.Authentication;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Infrastructure;

public sealed class PostgreSqlIntegrationFixture : IAsyncLifetime
{
    public const string CollectionName = "PostgreSqlIntegration";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("accounting_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public AppDbContext CreateDbContext(IUserContext? userContext = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
        var context = new AppDbContext(options);

        if (userContext is not null)
            context.SetUserContext(userContext);

        return context;
    }
}

[CollectionDefinition(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class PostgreSqlIntegrationCollection : ICollectionFixture<PostgreSqlIntegrationFixture>;
