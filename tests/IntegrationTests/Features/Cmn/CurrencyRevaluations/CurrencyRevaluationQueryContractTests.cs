using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.Cmn.CurrencyRevaluations;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace IntegrationTests.Features.Cmn.CurrencyRevaluations;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class CurrencyRevaluationQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int OrganizationId = 35001;
    private const int OtherOrganizationId = 35002;
    private const short BaseCurrencyId = 29001;
    private const short TargetCurrencyId = 29002;

    [Fact]
    public async Task ListPagesInSqlAndDetailReturnsLinesWithinOrganizationScope()
    {
        await SeedAsync();
        var user = new IntegrationTestUserContext
        {
            Id = 35001,
            UserKind = CurrentUserKind.TenantUser,
            LanguageId = 2,
            TenantId = 35001,
            OrganizationId = OrganizationId,
            AllowedOrganizationIds = [OrganizationId]
        };
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICurrencyRevaluationService>();

        var list = await service.GetAllAsync(new CurrencyRevaluationListFilter
        {
            Page = 1,
            PageSize = 1
        });
        var detail = await service.GetByIdAsync(35001);
        var inaccessible = await service.GetByIdAsync(35003);

        Assert.True(list.IsSuccess);
        Assert.Equal(2, list.Value.TotalCount);
        Assert.Equal(35001, Assert.Single(list.Value.Items).Id);
        Assert.Equal(1, list.Value.PageSize);

        Assert.True(detail.IsSuccess);
        var line = Assert.Single(detail.Value.Lines);
        Assert.Equal("RVTARGET", line.TargetCurrencyCode);
        Assert.Equal(125m, line.BalanceAmount);
        Assert.Equal(2.5m, line.OpeningRate);
        Assert.Equal(3m, line.CurrentRate);
        Assert.Equal(62.5m, line.DifferenceAmount);

        Assert.False(inaccessible.IsSuccess);
        Assert.Equal("CurrencyRevaluation.NotFound", inaccessible.Error.Code);
    }

    [Fact]
    public async Task ListAppliesDateAndIdSearchBeforeCountingAndKeepsDateDescendingOrder()
    {
        await SeedAsync();
        var user = new IntegrationTestUserContext
        {
            Id = 35001,
            UserKind = CurrentUserKind.TenantUser,
            OrganizationId = OrganizationId,
            AllowedOrganizationIds = [OrganizationId]
        };
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICurrencyRevaluationService>();

        var dateFiltered = await service.GetAllAsync(new CurrencyRevaluationListFilter
        {
            RevaluationFrom = new DateTime(2026, 9, 1),
            Page = 1,
            PageSize = 10
        });
        var idSearch = await service.GetAllAsync(new CurrencyRevaluationListFilter
        {
            Search = "35002",
            Page = 1,
            PageSize = 10
        });

        Assert.True(dateFiltered.IsSuccess);
        Assert.Equal([35001L], dateFiltered.Value.Items.Select(item => item.Id));
        Assert.Equal(1, dateFiltered.Value.TotalCount);
        Assert.True(idSearch.IsSuccess);
        Assert.Equal(35002, Assert.Single(idSearch.Value.Items).Id);
        Assert.Equal(1, idSearch.Value.TotalCount);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(fixture, user, services =>
        {
            services.AddLogging();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IDocumentPostingLock, NoOpDocumentPostingLock>();
            services.AddSingleton<IAccountingPeriodValidator, OpenAccountingPeriodValidator>();
            services.AddSingleton<IAccountingDispatcher, SuccessfulAccountingDispatcher>();
        });

    private async Task SeedAsync()
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

        if (!await context.Regions.AnyAsync(region => region.Id == 35001))
        {
            context.Regions.Add(new Region
            {
                Id = 35001,
                ShortName = "Revaluation test region",
                FullName = "Revaluation test region",
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == 35001))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = 35001,
                Name = "Revaluation query tenant",
                Slug = "revaluation-query-tenant",
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Revaluation organization", "350000001"),
                Organization(OtherOrganizationId, "Other revaluation organization", "350000002"));
        }

        if (!await context.Currencies.AnyAsync(currency => currency.Id == BaseCurrencyId))
        {
            context.Currencies.AddRange(
                Currency(BaseCurrencyId, "RVBASE", "Revaluation base currency"),
                Currency(TargetCurrencyId, "RVTARGET", "Revaluation target currency"));
        }

        if (!await context.CurrencyRevaluations.IgnoreQueryFilters().AnyAsync(item => item.Id == 35001))
        {
            context.CurrencyRevaluations.AddRange(
                Revaluation(35001, OrganizationId, new DateTime(2026, 9, 1), withLine: true),
                Revaluation(35002, OrganizationId, new DateTime(2026, 8, 31), withLine: false),
                Revaluation(35003, OtherOrganizationId, new DateTime(2026, 9, 2), withLine: false));
        }

        await context.SaveChangesAsync();
    }

    private static Organization Organization(int id, string name, string inn) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = inn,
        RegionId = 35001,
        IsParent = false,
        StateId = 1,
        CreatedDate = SeedDate,
        TenantId = 35001,
        SetupStatus = "completed"
    };

    private static Currency Currency(short id, string code, string name) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        Symbol = code,
        StateId = 1
    };

    private static CurrencyRevaluation Revaluation(long id, int organizationId, DateTime date, bool withLine)
    {
        var entity = new CurrencyRevaluation
        {
            Id = id,
            OrganizationId = organizationId,
            RevaluationDate = date,
            ProviderRateDate = date,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = SeedDate
        };

        if (withLine)
        {
            entity.Lines.Add(new CurrencyRevaluationLine
            {
                Id = 35001,
                RevaluationId = id,
                BaseCurrencyId = BaseCurrencyId,
                TargetCurrencyId = TargetCurrencyId,
                BalanceAmount = 125m,
                OpeningRate = 2.5m,
                CurrentRate = 3m,
                DifferenceAmount = 62.5m,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        return entity;
    }

    private sealed class NoOpDocumentPostingLock : IDocumentPostingLock
    {
        public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
        public Task AcquireInventoryAsync(int organizationId, int warehouseId, IReadOnlyCollection<int> productIds, IReadOnlyCollection<int> productTableIds, CancellationToken ct = default) => Task.CompletedTask;
        public Task AcquireMoneyAsync(int organizationId, string sourceType, int sourceId, CancellationToken ct = default) => Task.CompletedTask;
        public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.FromResult(true);
    }

    private sealed class OpenAccountingPeriodValidator : IAccountingPeriodValidator
    {
        public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) =>
            Task.FromResult(Result.Success());
    }

    private sealed class SuccessfulAccountingDispatcher : IAccountingDispatcher
    {
        public Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null) =>
            Task.FromResult(Result.Success(new List<AccountingRegisterEntry>()));
    }
}
