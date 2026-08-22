using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class ContractOrganizationIsolationContractTests
{
    [Fact]
    public async Task BackgroundOrganizationScopeExposesOnlyItsContracts()
    {
        var databaseName = $"contract-isolation-{Guid.NewGuid():N}";
        var scope = new BackgroundOrganizationScope();

        await AddContractAsync(databaseName, scope, 2, 20);
        await AddContractAsync(databaseName, scope, 3, 30);

        using (scope.Enter(2, "contract-isolation-test"))
        await using (var context = CreateContext(databaseName, scope))
        {
            var contracts = await context.Contracts.AsNoTracking().ToListAsync();

            Assert.Single(contracts);
            Assert.Equal(2, contracts[0].OrganizationId);
        }

        using (scope.Enter(3, "contract-isolation-test"))
        await using (var context = CreateContext(databaseName, scope))
        {
            var contracts = await context.Contracts.AsNoTracking().ToListAsync();

            Assert.Single(contracts);
            Assert.Equal(3, contracts[0].OrganizationId);
        }
    }

    private static async Task AddContractAsync(
        string databaseName,
        BackgroundOrganizationScope scope,
        int organizationId,
        long id)
    {
        using (scope.Enter(organizationId, "contract-isolation-seed"))
        await using (var context = CreateContext(databaseName, scope))
        {
            context.Contracts.Add(new Contract
            {
                Id = id,
                OrganizationId = organizationId,
                CounterpartyId = organizationId,
                ContractTypeId = 1,
                ContractNumber = $"LOCAL-{organizationId}",
                ContractDate = new DateTime(2026, 1, 1),
                StateId = 1,
                CreatedDate = new DateTime(2026, 1, 1)
            });

            await context.SaveChangesAsync();
        }
    }

    private static AppDbContext CreateContext(
        string databaseName,
        BackgroundOrganizationScope scope) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options, scope);
}
