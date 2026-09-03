using Application.Features.Banks;
using Domain.Entities;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Features.Cmn.Banks;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class BankQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);

    [Fact]
    public async Task ListAndDetailKeepBaseNamesAndApplySearchOrderAndPaging()
    {
        await SeedBanksAsync();
        var user = new IntegrationTestUserContext { LanguageId = 2 };
        await using var provider = ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<IBankService, BankService>());
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBankService>();

        var page = await service.GetAllAsync(new BankListFilter
        {
            Search = "contract bank",
            Page = 2,
            PageSize = 1
        });
        var detail = await service.GetByIdAsync(32001);

        Assert.True(page.IsSuccess);
        Assert.Equal(2, page.Value.TotalCount);
        Assert.Equal("Beta contract bank", Assert.Single(page.Value.Items).Name);
        Assert.True(detail.IsSuccess);
        Assert.Equal("Alpha contract bank", detail.Value.Name);
        Assert.Equal("BNKA", detail.Value.Code);
    }

    [Fact]
    public async Task BranchQueriesTrimMfoOrderNamesAndReturnLocalizedErrors()
    {
        await SeedBanksAsync();
        var user = new IntegrationTestUserContext { LanguageId = 2 };
        await using var provider = ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<IBankService, BankService>());
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBankService>();

        var branches = await service.GetBranchesAsync(32001);
        var branchByMfo = await service.GetBranchByMfoAsync(" 10001 ");
        var missingBank = await service.GetBranchesAsync(32999);
        var missingBranch = await service.GetBranchByMfoAsync(" 99999 ");

        Assert.True(branches.IsSuccess);
        Assert.Equal(["First branch", "Second branch"], branches.Value.Select(branch => branch.Name));
        Assert.True(branchByMfo.IsSuccess);
        Assert.Equal(32011, branchByMfo.Value.Id);
        Assert.Equal("Alpha contract bank", branchByMfo.Value.BankName);

        Assert.False(missingBank.IsSuccess);
        Assert.Equal("Bank.NotFound", missingBank.Error.Code);
        Assert.Equal("Банк с id 32999 не найден.", missingBank.Error.Description);
        Assert.False(missingBranch.IsSuccess);
        Assert.Equal("BankBranch.NotFound", missingBranch.Error.Code);
        Assert.Equal("Филиал банка с МФО '99999' не найден.", missingBranch.Error.Description);
    }

    private async Task SeedBanksAsync()
    {
        await using var context = fixture.CreateDbContext();

        if (!await context.States.AnyAsync(state => state.Id == 1))
        {
            context.States.Add(new State
            {
                Id = 1,
                ShortName = "Active",
                FullName = "Active",
                CreatedDate = SeedDate
            });
        }

        if (!await context.Banks.AnyAsync(bank => bank.Id == 32001))
        {
            context.Banks.AddRange(
                new Bank
                {
                    Id = 32001,
                    Code = "BNKA",
                    Name = "Alpha contract bank",
                    LegalName = "Alpha legal name",
                    Inn = "300000001",
                    StateId = 1,
                    CreatedDate = SeedDate
                },
                new Bank
                {
                    Id = 32002,
                    Code = "BNKB",
                    Name = "Beta contract bank",
                    LegalName = "Beta legal name",
                    Inn = "300000002",
                    StateId = 1,
                    CreatedDate = SeedDate
                });

            context.BankBranches.AddRange(
                new BankBranch
                {
                    Id = 32011,
                    BankId = 32001,
                    Mfo = "10001",
                    BranchType = 1,
                    Name = "First branch",
                    StateId = 1,
                    CreatedDate = SeedDate
                },
                new BankBranch
                {
                    Id = 32012,
                    BankId = 32001,
                    Mfo = "10002",
                    BranchType = 1,
                    Name = "Second branch",
                    StateId = 1,
                    CreatedDate = SeedDate
                });
        }

        await context.SaveChangesAsync();
    }
}
