using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.OrganizationSetup;
using Domain.Entities;
using Infrastructure.Repositories;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.OrganizationManagement.Setup;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class OrganizationSetupQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 44001;
    private const int OrganizationId = 44001;
    private const int OtherOrganizationId = 44002;
    private const int ReadyOrganizationId = 44003;
    private const int RegionId = 44001;
    private const int BranchId = 44101;
    private const short OlderTaxTypeId = 29401;
    private const short CurrentTaxTypeId = 29402;
    private const short PricingMethodId = 29401;
    private const short RoundingMethodId = 29401;

    [Fact]
    public async Task GetMapsSelectedOrganizationAggregateWithoutCrossOrganizationData()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(CurrentUserKind.SuperAdmin, OrganizationId, 44001));
        await using var scope = provider.CreateAsyncScope();

        var result = await scope.ServiceProvider.GetRequiredService<IOrganizationSetupService>().GetAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(OrganizationId, result.Value.OrganizationId);
        Assert.Equal("Setup organization", result.Value.CompanyProfile.ShortName);
        Assert.Equal("company-profile", result.Value.CurrentStep);
        Assert.NotNull(result.Value.TaxSettings);
        Assert.Equal(CurrentTaxTypeId, result.Value.TaxSettings.TaxTypeId);
        Assert.Equal(new DateOnly(2026, 1, 1), result.Value.TaxSettings.EffectiveFrom);
        Assert.NotNull(result.Value.AccountingPolicy);
        Assert.Equal("fifo", result.Value.AccountingPolicy.InventoryValuationMethod);
        Assert.Equal((short)4, result.Value.AccountingPolicy.FiscalYearStartMonth);
        Assert.NotNull(result.Value.Defaults);
        Assert.Equal(BranchId, result.Value.Defaults.BranchId);
        Assert.NotNull(result.Value.PricingCondition);
        Assert.Equal(44202, result.Value.PricingCondition.Id);
        Assert.Equal("Markup", result.Value.PricingCondition.PricingMethodName);
    }

    [Fact]
    public async Task GetReturnsLocalizedErrorsForMissingContextAndMissingMembership()
    {
        await SeedAsync();

        await using var missingProvider = CreateProvider(User(CurrentUserKind.SuperAdmin, null, 44001));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missing = await missingScope.ServiceProvider.GetRequiredService<IOrganizationSetupService>().GetAsync();

        await using var forbiddenProvider = CreateProvider(User(CurrentUserKind.TenantUser, OrganizationId, 44999));
        await using var forbiddenScope = forbiddenProvider.CreateAsyncScope();
        var forbidden = await forbiddenScope.ServiceProvider.GetRequiredService<IOrganizationSetupService>().GetAsync();

        Assert.False(missing.IsSuccess);
        Assert.Equal("OrganizationSetup.OrganizationContextRequired", missing.Error.Code);
        Assert.Contains("Требуется контекст организации", missing.Error.Description);
        Assert.False(forbidden.IsSuccess);
        Assert.Equal("OrganizationSetup.Forbidden", forbidden.Error.Code);
        Assert.Equal("У вас нет доступа к управлению настройкой этой организации.", forbidden.Error.Description);
    }

    [Fact]
    public async Task CompleteCommitsReadySetupAndRejectsIncompleteSetup()
    {
        await SeedAsync();

        await using var readyProvider = CreateProvider(User(CurrentUserKind.SuperAdmin, ReadyOrganizationId, 44001));
        await using var readyScope = readyProvider.CreateAsyncScope();
        var completed = await readyScope.ServiceProvider.GetRequiredService<IOrganizationSetupService>().CompleteAsync();

        await using var incompleteProvider = CreateProvider(User(CurrentUserKind.SuperAdmin, OtherOrganizationId, 44001));
        await using var incompleteScope = incompleteProvider.CreateAsyncScope();
        var incomplete = await incompleteScope.ServiceProvider.GetRequiredService<IOrganizationSetupService>().CompleteAsync();

        Assert.True(completed.IsSuccess);
        Assert.False(incomplete.IsSuccess);
        Assert.Equal("OrganizationSetup.NotReady", incomplete.Error.Code);

        await using var context = fixture.CreateDbContext();
        var organization = await context.Organizations.IgnoreQueryFilters()
            .SingleAsync(item => item.Id == ReadyOrganizationId);
        var setup = await context.OrganizationSetupStates.IgnoreQueryFilters()
            .SingleAsync(item => item.OrganizationId == ReadyOrganizationId);
        var incompleteOrganization = await context.Organizations.IgnoreQueryFilters()
            .SingleAsync(item => item.Id == OtherOrganizationId);

        Assert.Equal("completed", organization.SetupStatus);
        Assert.True(setup.IsCompleted);
        Assert.Equal("complete", setup.CurrentStep);
        Assert.NotNull(setup.CompletedAt);
        Assert.Equal("pending", incompleteOrganization.SetupStatus);
    }

    [Fact]
    public async Task DefaultsRejectChartAccountFromAnotherOrganization()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(CurrentUserKind.SuperAdmin, OrganizationId, 44001));
        await using var scope = provider.CreateAsyncScope();

        var result = await scope.ServiceProvider
            .GetRequiredService<IOrganizationSetupService>()
            .UpdateDefaultsAsync(new OrganizationSetupDefaultsDto
            {
                ReceivableAccountId = 44502
            });

        Assert.False(result.IsSuccess);
        Assert.Equal("OrganizationSetup.ChartAccountNotFound", result.Error.Code);
        Assert.Equal("Бухгалтерский счёт с id 44502 не найден в текущей организации.", result.Error.Description);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services =>
            {
                services.AddLogging();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<IOrganizationSetupCore, OrganizationSetupCore>();
                services.AddScoped<IOrganizationSetupService, OrganizationSetupService>();
            });

    private static IntegrationTestUserContext User(CurrentUserKind kind, int? organizationId, int userId) => new()
    {
        Id = userId,
        UserKind = kind,
        LanguageId = LanguageIdConst.RU,
        TenantId = TenantId,
        OrganizationId = organizationId,
        AllowedOrganizationIds = organizationId.HasValue ? [organizationId.Value] : []
    };

    private async Task SeedAsync()
    {
        await using var context = fixture.CreateDbContext();

        if (!await context.States.AnyAsync(state => state.Id == StateIdConst.ACTIVE))
        {
            context.States.Add(new State
            {
                Id = StateIdConst.ACTIVE,
                ShortName = "Active",
                FullName = "Active",
                CreatedDate = SeedDate
            });
        }

        if (!await context.Regions.AnyAsync(region => region.Id == RegionId))
        {
            context.Regions.Add(new Region
            {
                Id = RegionId,
                ShortName = "Setup region",
                FullName = "Setup region",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = TenantId,
                Name = "Setup tenant",
                Slug = "organization-setup-query-tenant",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Setup organization"),
                Organization(OtherOrganizationId, "Incomplete setup organization"),
                Organization(ReadyOrganizationId, "Ready setup organization"));
        }

        if (!await context.Branches.IgnoreQueryFilters().AnyAsync(branch => branch.Id == BranchId))
        {
            context.Branches.Add(new Branch
            {
                Id = BranchId,
                OrganizationId = OrganizationId,
                Code = "SETUP-BRANCH",
                Name = "Setup branch",
                RegionId = RegionId,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.TaxTypes.AnyAsync(taxType => taxType.Id == OlderTaxTypeId))
        {
            context.TaxTypes.AddRange(
                TaxType(OlderTaxTypeId, "SETUP-OLD", "Older setup tax"),
                TaxType(CurrentTaxTypeId, "SETUP-CURRENT", "Current setup tax"));
        }

        if (!await context.PricingMethods.AnyAsync(method => method.Id == PricingMethodId))
        {
            context.PricingMethods.Add(new PricingMethod
            {
                Id = PricingMethodId,
                Code = "SETUP-MARKUP",
                Name = "Markup"
            });
            context.PriceRoundingMethods.Add(new PriceRoundingMethod
            {
                Id = RoundingMethodId,
                Code = "SETUP-ROUND",
                Name = "Round down"
            });
        }

        if (!await context.OrganizationSetupStates.IgnoreQueryFilters().AnyAsync(setup => setup.OrganizationId == OrganizationId))
        {
            context.OrganizationSetupStates.AddRange(
                SetupState(OrganizationId, "company-profile", true, false, false, false),
                SetupState(OtherOrganizationId, "tax-settings", true, false, false, false),
                SetupState(ReadyOrganizationId, "complete", true, true, true, true));
        }

        if (!await context.OrganizationTaxSettings.IgnoreQueryFilters().AnyAsync(setting => setting.OrganizationId == OrganizationId))
        {
            context.OrganizationTaxSettings.AddRange(
                TaxSetting(44301, OrganizationId, OlderTaxTypeId, new DateOnly(2025, 1, 1)),
                TaxSetting(44302, OrganizationId, CurrentTaxTypeId, new DateOnly(2026, 1, 1)),
                TaxSetting(44303, OtherOrganizationId, OlderTaxTypeId, new DateOnly(2026, 8, 1)));
        }

        if (!await context.OrganizationConfigs.IgnoreQueryFilters().AnyAsync(config => config.OrganizationId == OrganizationId))
        {
            context.OrganizationConfigs.AddRange(
                new OrganizationConfig
                {
                    OrganizationId = OrganizationId,
                    InventoryValuationMethod = "fifo",
                    FiscalYearStartMonth = 4
                },
                new OrganizationConfig
                {
                    OrganizationId = OtherOrganizationId,
                    InventoryValuationMethod = "average",
                    FiscalYearStartMonth = 7
                });
        }

        if (!await context.OrganizationDefaults.IgnoreQueryFilters().AnyAsync(defaults => defaults.OrganizationId == OrganizationId))
        {
            context.OrganizationDefaults.Add(new OrganizationDefault
            {
                Id = 44401,
                OrganizationId = OrganizationId,
                BranchId = BranchId,
                CreatedDate = SeedDate
            });
        }

        if (!await context.ChartAccounts.IgnoreQueryFilters().AnyAsync(account => account.Id == 44501))
        {
            context.ChartAccounts.AddRange(
                ChartAccount(44501, OrganizationId, "1000", "Own setup account"),
                ChartAccount(44502, OtherOrganizationId, "2000", "Other setup account"));
        }

        if (!await context.PricingConditions.IgnoreQueryFilters().AnyAsync(condition => condition.Id == 44201))
        {
            context.PricingConditions.AddRange(
                PricingCondition(44201, OrganizationId, SeedDate.AddDays(-10)),
                PricingCondition(44202, OrganizationId, SeedDate.AddDays(-1)),
                PricingCondition(44203, OtherOrganizationId, SeedDate.AddHours(-1)));
        }

        await context.SaveChangesAsync();
    }

    private static Organization Organization(int id, string name) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = id.ToString(),
        RegionId = RegionId,
        IsParent = false,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate,
        TenantId = TenantId,
        SetupStatus = "pending"
    };

    private static TaxType TaxType(short id, string code, string name) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static OrganizationSetupState SetupState(
        int organizationId,
        string currentStep,
        bool organizationCompleted,
        bool taxCompleted,
        bool accountingCompleted,
        bool defaultsCompleted) => new()
    {
        OrganizationId = organizationId,
        CurrentStep = currentStep,
        OrganizationCompleted = organizationCompleted,
        TaxCompleted = taxCompleted,
        AccountingCompleted = accountingCompleted,
        DefaultsCompleted = defaultsCompleted,
        IsCompleted = false,
        UpdatedDate = SeedDate,
        CreatedDate = SeedDate
    };

    private static OrganizationTaxSetting TaxSetting(int id, int organizationId, short taxTypeId, DateOnly effectiveFrom) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        TaxTypeId = taxTypeId,
        IsVatPayer = true,
        EffectiveFrom = effectiveFrom,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static PricingCondition PricingCondition(long id, int organizationId, DateTime startDate) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        PricingMethodId = PricingMethodId,
        PricingValue = id,
        RoundingMethodId = RoundingMethodId,
        RoundingPrecision = 1,
        StartDate = startDate,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static ChartAccount ChartAccount(int id, int organizationId, string number, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        Number = number,
        Name = name,
        IsGroup = false,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };
}
