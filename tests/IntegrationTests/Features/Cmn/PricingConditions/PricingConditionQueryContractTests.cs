using Application.Abstractions.Authentication;
using Application.Features.PricingConditions;
using Domain.Entities;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace IntegrationTests.Features.Cmn.PricingConditions;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class PricingConditionQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int OrganizationId = 37001;
    private const int OtherOrganizationId = 37002;
    private const short PricingMethodId = 27001;
    private const short RoundingMethodId = 27001;

    [Fact]
    public async Task ListUsesBaseNamesAndAppliesScopeSearchOrderAndPagingInSql()
    {
        await SeedAsync();
        var user = User();
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IPricingConditionService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var projection = scope.ServiceProvider
            .GetRequiredService<IProjectionBuilder<PricingCondition, PricingConditionListDto>>()
            .Build();

        var sql = context.PricingConditions.Select(projection).ToQueryString();
        var result = await service.GetAllAsync(new PricingConditionListFilter
        {
            StateId = StateIdConst.ACTIVE,
            Search = "markup",
            Page = 1,
            PageSize = 1
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        var condition = Assert.Single(result.Value.Items);
        Assert.Equal(37002, condition.Id);
        Assert.Equal("Markup method", condition.PricingMethodName);
        Assert.Equal("Round down", condition.RoundingMethodName);
        Assert.DoesNotContain("_translation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(result.Value.Items, item => item.OrganizationId != OrganizationId);
    }

    [Fact]
    public async Task CurrentSelectsMostRecentActiveOverlapAndMissingDetailUsesRequestedLanguage()
    {
        await SeedAsync();
        var user = User();
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IPricingConditionService>();

        var current = await service.GetNowAsync();
        var detail = await service.GetByIdAsync(37001);
        var missing = await service.GetByIdAsync(37999);

        Assert.True(current.IsSuccess);
        Assert.Equal(37002, current.Value.Id);
        Assert.True(detail.IsSuccess);
        Assert.Equal("Markup method", detail.Value.PricingMethodName);
        Assert.False(missing.IsSuccess);
        Assert.Equal("PricingCondition.NotFound", missing.Error.Code);
        Assert.Equal("Условие ценообразования с id 37999 не найдено.", missing.Error.Description);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<IPricingConditionService, PricingConditionService>());

    private static IntegrationTestUserContext User() => new()
    {
        Id = 37001,
        UserKind = CurrentUserKind.TenantUser,
        LanguageId = LanguageIdConst.RU,
        TenantId = 37001,
        OrganizationId = OrganizationId,
        AllowedOrganizationIds = [OrganizationId]
    };

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

        if (!await context.States.AnyAsync(state => state.Id == StateIdConst.PASSIVE))
        {
            context.States.Add(new State
            {
                Id = StateIdConst.PASSIVE,
                ShortName = "Passive",
                FullName = "Passive",
                CreatedDate = SeedDate
            });
        }

        if (!await context.Regions.AnyAsync(region => region.Id == 37001))
        {
            context.Regions.Add(new Region
            {
                Id = 37001,
                ShortName = "Pricing condition region",
                FullName = "Pricing condition region",
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == 37001))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = 37001,
                Name = "Pricing condition tenant",
                Slug = "pricing-condition-query-tenant",
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Pricing condition organization", "370000001"),
                Organization(OtherOrganizationId, "Other pricing organization", "370000002"));
        }

        if (!await context.PricingMethods.AnyAsync(method => method.Id == PricingMethodId))
        {
            context.PricingMethods.Add(new PricingMethod
            {
                Id = PricingMethodId,
                Code = "MARKUP-TEST",
                Name = "Markup method"
            });
        }

        if (!await context.PriceRoundingMethods.AnyAsync(method => method.Id == RoundingMethodId))
        {
            context.PriceRoundingMethods.Add(new PriceRoundingMethod
            {
                Id = RoundingMethodId,
                Code = "ROUND-DOWN-TEST",
                Name = "Round down"
            });
        }

        if (!await context.PricingConditions.IgnoreQueryFilters().AnyAsync(condition => condition.Id == 37001))
        {
            var now = DateTime.Now;
            context.PricingConditions.AddRange(
                Condition(37001, OrganizationId, now.AddDays(-5), now.AddDays(5), StateIdConst.ACTIVE),
                Condition(37002, OrganizationId, now.AddDays(-1), null, StateIdConst.ACTIVE),
                Condition(37003, OrganizationId, now.AddHours(-12), null, StateIdConst.PASSIVE),
                Condition(37004, OtherOrganizationId, now.AddHours(-1), null, StateIdConst.ACTIVE));
        }

        await context.SaveChangesAsync();
    }

    private static Organization Organization(int id, string name, string inn) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = inn,
        RegionId = 37001,
        IsParent = false,
        StateId = 1,
        CreatedDate = SeedDate,
        TenantId = 37001,
        SetupStatus = "completed"
    };

    private static PricingCondition Condition(
        long id,
        int organizationId,
        DateTime startDate,
        DateTime? endDate,
        short stateId) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        PricingMethodId = PricingMethodId,
        PricingValue = id,
        RoundingMethodId = RoundingMethodId,
        RoundingPrecision = 1m,
        StartDate = startDate,
        EndDate = endDate,
        StateId = stateId,
        CreatedDate = SeedDate
    };
}
