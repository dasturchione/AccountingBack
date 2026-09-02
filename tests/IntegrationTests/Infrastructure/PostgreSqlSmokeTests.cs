using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Infrastructure;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class PostgreSqlSmokeTests(PostgreSqlIntegrationFixture fixture)
{
    [Fact]
    public async Task AppDbContextExecutesAgainstPostgreSql()
    {
        await using var context = fixture.CreateDbContext();

        Assert.True(await context.Database.CanConnectAsync());
        Assert.Contains("Npgsql", context.Database.ProviderName);
    }
}
