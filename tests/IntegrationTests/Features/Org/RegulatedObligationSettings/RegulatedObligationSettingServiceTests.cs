using Application.Abstractions.Authentication;
using Application.Features.RegulatedObligationSettings;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Query;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.Org.RegulatedObligationSettings;

public sealed class RegulatedObligationSettingServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsEveryObligationAndOnlySettingEffectiveOnChoosedDate()
    {
        await using var context = CreateContext();
        SeedReferenceData(context);
        context.OrganizationRegulatedObligationSettings.Add(new OrganizationRegulatedObligationSetting
        {
            Id = 100,
            OrganizationId = 7,
            RegulatedObligationId = 10,
            PeriodicityId = 20,
            ClassifierCode = "7",
            Rate = 12,
            ChartAccountId = 30,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            StateId = StateIdConst.ACTIVE,
            CreatedDate = new DateTime(2026, 1, 1)
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetAllAsync(new RegulatedObligationSettingListFilter
        {
            ChoosedDate = new DateOnly(2026, 6, 1)
        });

        Assert.True(result.IsSuccess);
        Assert.Collection(
            result.Value,
            excise =>
            {
                Assert.Equal("EXCISE_TAX", excise.Code);
                Assert.Null(excise.SettingId);
            },
            vat =>
            {
                Assert.Equal("VAT", vat.Code);
                Assert.Equal(100, vat.SettingId);
                Assert.Equal(12, vat.Rate);
                Assert.Equal("MONTHLY", vat.PeriodicityCode);
            });
    }

    [Fact]
    public async Task GetAllAsync_AppliesCategorySearchAndConfiguredFilters()
    {
        await using var context = CreateContext();
        SeedReferenceData(context);
        context.RegulatedObligationCategories.Add(new RegulatedObligationCategory
        {
            Id = 2,
            Code = "CONTRIBUTION",
            Name = "Ajratma",
            StateId = StateIdConst.ACTIVE
        });
        context.RegulatedObligations.Add(new RegulatedObligation
        {
            Id = 12,
            CategoryId = 2,
            Code = "EXCISE_CONTRIBUTION",
            Name = "Excise contribution",
            StateId = StateIdConst.ACTIVE
        });
        context.OrganizationRegulatedObligationSettings.Add(CreateSetting(100, 7));
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var notConfigured = await service.GetAllAsync(new RegulatedObligationSettingListFilter
        {
            CategoryCode = " tax ",
            ChoosedDate = new DateOnly(2026, 6, 1),
            Search = "excise",
            IsConfigured = false
        });
        var configured = await service.GetAllAsync(new RegulatedObligationSettingListFilter
        {
            CategoryCode = "TAX",
            ChoosedDate = new DateOnly(2026, 6, 1),
            IsConfigured = true
        });

        Assert.True(notConfigured.IsSuccess);
        Assert.Collection(notConfigured.Value, item => Assert.Equal("EXCISE_TAX", item.Code));
        Assert.True(configured.IsSuccess);
        Assert.Collection(configured.Value, item => Assert.Equal("VAT", item.Code));
    }

    [Fact]
    public async Task CreateAsync_RejectsAnOverlappingPeriodForSameObligation()
    {
        await using var context = CreateContext();
        SeedReferenceData(context);
        context.OrganizationRegulatedObligationSettings.Add(new OrganizationRegulatedObligationSetting
        {
            Id = 100,
            OrganizationId = 7,
            RegulatedObligationId = 10,
            PeriodicityId = 20,
            ChartAccountId = 30,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            StateId = StateIdConst.ACTIVE,
            CreatedDate = new DateTime(2026, 1, 1)
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.CreateAsync(new RegulatedObligationSettingCreateDto
        {
            RegulatedObligationId = 10,
            PeriodicityId = 20,
            ChartAccountId = 30,
            Rate = 15,
            EffectiveFrom = new DateOnly(2026, 12, 31),
            StateId = StateIdConst.ACTIVE
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("RegulatedObligationSetting.PeriodOverlap", result.Error.Code);
        Assert.Single(context.OrganizationRegulatedObligationSettings);
    }

    [Fact]
    public async Task GetByCodeAsync_ReturnsObligationWithNullSettingWhenOrganizationHasNoConfiguration()
    {
        await using var context = CreateContext();
        SeedReferenceData(context);

        var result = await CreateService(context).GetByCodeAsync("vat", new DateOnly(2026, 6, 1));

        Assert.True(result.IsSuccess);
        Assert.Equal("VAT", result.Value.Code);
        Assert.Equal("НДС", result.Value.Name);
        Assert.Null(result.Value.SettingId);
        Assert.Equal(7, result.Value.OrganizationId);
    }

    [Fact]
    public async Task CreateAndUpdate_PersistValuesReturnedByGetById()
    {
        await using var context = CreateContext();
        SeedReferenceData(context);
        var service = CreateService(context);

        var created = await service.CreateAsync(new RegulatedObligationSettingCreateDto
        {
            RegulatedObligationId = 10,
            PeriodicityId = 20,
            ClassifierCode = " 7 ",
            Rate = 12,
            ChartAccountId = 30,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            StateId = StateIdConst.ACTIVE
        });
        Assert.True(created.IsSuccess);

        var updated = await service.UpdateAsync(created.Value, new RegulatedObligationSettingUpdateDto
        {
            RegulatedObligationId = 10,
            PeriodicityId = 20,
            ClassifierCode = "007",
            Rate = 15,
            ChartAccountId = 30,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2026, 11, 30),
            StateId = StateIdConst.ACTIVE
        });
        Assert.True(updated.IsSuccess);

        var result = await service.GetByIdAsync(created.Value);

        Assert.True(result.IsSuccess);
        Assert.Equal("007", result.Value.ClassifierCode);
        Assert.Equal(15, result.Value.Rate);
        Assert.Equal(new DateOnly(2026, 11, 30), result.Value.EffectiveTo);
        Assert.NotNull(result.Value.UpdatedDate);
    }

    [Fact]
    public async Task AccessScope_HidesSettingsOfAnotherOrganization()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        await using var context = new AppDbContext(options);
        SeedReferenceData(context);
        context.OrganizationRegulatedObligationSettings.AddRange(
            CreateSetting(100, 7),
            CreateSetting(101, 8));
        await context.SaveChangesAsync();
        context.SetUserContext(TestUserContext.Instance);

        var ids = await context.OrganizationRegulatedObligationSettings.Select(x => x.Id).ToListAsync();

        Assert.Equal([100], ids);
    }

    private static AppDbContext CreateContext()
    {
        var userContext = TestUserContext.Instance;
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        var context = new AppDbContext(options);
        context.SetUserContext(userContext);
        return context;
    }

    private static RegulatedObligationSettingService CreateService(AppDbContext context)
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var queryBuilder = new QueryBuilder(new QueryBuilderResolver(serviceProvider));

        return new RegulatedObligationSettingService(
            TestUserContext.Instance,
            queryBuilder,
            new QueryRepository<RegulatedObligation>(context),
            new QueryRepository<OrganizationRegulatedObligationSetting>(context),
            new QueryRepository<RegulatedObligationPeriodicity>(context),
            new QueryRepository<ChartAccount>(context),
            new CommandRepository<OrganizationRegulatedObligationSetting>(context));
    }

    private static void SeedReferenceData(AppDbContext context)
    {
        context.States.Add(new State { Id = StateIdConst.ACTIVE, ShortName = "Active", FullName = "Active" });
        context.Languages.Add(new Language { Id = 2, Code = "ru", Name = "Russian", NativeName = "Русский", StateId = StateIdConst.ACTIVE });
        context.Organizations.Add(new Organization
        {
            Id = 7,
            ShortName = "Test",
            FullName = "Test organization",
            Inn = "123456789",
            RegionId = 1,
            StateId = StateIdConst.ACTIVE,
            TenantId = 1,
            SetupStatus = "completed"
        });
        context.ChartAccounts.Add(new ChartAccount
        {
            Id = 30,
            OrganizationId = 7,
            Number = "6410",
            Name = "Taxes",
            StateId = StateIdConst.ACTIVE
        });

        var category = new RegulatedObligationCategory
        {
            Id = 1,
            Code = "TAX",
            Name = "Soliq",
            StateId = StateIdConst.ACTIVE,
            Translations =
            [
                new RegulatedObligationCategoryTranslation { CategoryId = 1, LanguageId = 2, Name = "Налог" }
            ]
        };
        context.RegulatedObligationCategories.Add(category);
        context.RegulatedObligations.AddRange(
            new RegulatedObligation
            {
                Id = 11,
                CategoryId = 1,
                Code = "EXCISE_TAX",
                Name = "Aksiz solig'i",
                StateId = StateIdConst.ACTIVE,
                Translations =
                [
                    new RegulatedObligationTranslation { RegulatedObligationId = 11, LanguageId = 2, Name = "Акцизный налог" }
                ]
            },
            new RegulatedObligation
            {
                Id = 10,
                CategoryId = 1,
                Code = "VAT",
                Name = "QQS",
                StateId = StateIdConst.ACTIVE,
                Translations =
                [
                    new RegulatedObligationTranslation { RegulatedObligationId = 10, LanguageId = 2, Name = "НДС" }
                ]
            });
        context.RegulatedObligationPeriodicities.Add(new RegulatedObligationPeriodicity
        {
            Id = 20,
            Code = "MONTHLY",
            Name = "Oy",
            StateId = StateIdConst.ACTIVE,
            Translations =
            [
                new RegulatedObligationPeriodicityTranslation { PeriodicityId = 20, LanguageId = 2, Name = "Месяц" }
            ]
        });
        context.SaveChanges();
    }

    private static OrganizationRegulatedObligationSetting CreateSetting(int id, int organizationId) =>
        new()
        {
            Id = id,
            OrganizationId = organizationId,
            RegulatedObligationId = 10,
            PeriodicityId = 20,
            ChartAccountId = 30,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            StateId = StateIdConst.ACTIVE,
            CreatedDate = new DateTime(2026, 1, 1)
        };

    private sealed class TestUserContext : IUserContext
    {
        public static readonly TestUserContext Instance = new();
        public int? Id => 1;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => 2;
        public int? TenantId => 1;
        public int? OrganizationId => 7;
        public List<int> AllowedOrganizationIds => [7];
        public int? BranchId => null;
    }
}
