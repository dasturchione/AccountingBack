using Application.Features.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;

namespace IntegrationTests.Features.Cmn.Contracts;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class ContractMultilanguageQueryTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int OrganizationId = 33001;
    private const int OtherOrganizationId = 33002;
    private const short TranslatedContractTypeId = 32101;
    private const short FallbackContractTypeId = 32102;

    [Fact]
    public async Task ListAndDetailUseRequestedContractTypeTranslationWithBaseFallbackInSql()
    {
        await SeedContractsAsync();
        var user = User(languageId: 2);
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IContractService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var projection = scope.ServiceProvider
            .GetRequiredService<IProjectionBuilder<Contract, ContractListDto>>()
            .Build();

        var sql = context.Contracts.Select(projection).ToQueryString();
        var translatedDetail = await service.GetByIdAsync(33001);
        var fallbackDetail = await service.GetByIdAsync(33004);

        Assert.True(translatedDetail.IsSuccess);
        Assert.Equal("Переведённый договор", translatedDetail.Value.ContractTypeName);
        Assert.True(fallbackDetail.IsSuccess);
        Assert.Equal("Base fallback contract", fallbackDetail.Value.ContractTypeName);
        Assert.Contains("cmn_contract_type_translation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke(", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ScopedProjectionUsesASecondRequestedLanguage()
    {
        await SeedContractsAsync();
        var user = User(languageId: 1);
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IContractService>();

        var result = await service.GetByIdAsync(33001);

        Assert.True(result.IsSuccess);
        Assert.Equal("Tarjima qilingan shartnoma", result.Value.ContractTypeName);
    }

    [Fact]
    public async Task TranslatedSearchOrdersPagesAndAppliesOrganizationScopeWithoutDuplicates()
    {
        await SeedContractsAsync();
        var user = User(languageId: 2);
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IContractService>();

        var secondPage = await service.GetAllAsync(new ContractListFilter
        {
            Search = "переведённый договор",
            Page = 2,
            PageSize = 1
        });
        var baseNameSearch = await service.GetAllAsync(new ContractListFilter
        {
            Search = "base translated contract",
            Page = 1,
            PageSize = 10
        });
        var inaccessibleDetail = await service.GetByIdAsync(33005);

        Assert.True(secondPage.IsSuccess);
        Assert.Equal(3, secondPage.Value.TotalCount);
        Assert.Equal(2, secondPage.Value.Page);
        Assert.Equal(1, secondPage.Value.PageSize);
        Assert.Equal(33002, Assert.Single(secondPage.Value.Items).Id);
        Assert.Equal(
            secondPage.Value.Items.Count,
            secondPage.Value.Items.Select(contract => contract.Id).Distinct().Count());

        Assert.True(baseNameSearch.IsSuccess);
        Assert.Empty(baseNameSearch.Value.Items);
        Assert.False(inaccessibleDetail.IsSuccess);
        Assert.Equal("Contract.NotFound", inaccessibleDetail.Error.Code);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<IContractService, ContractService>());

    private static IntegrationTestUserContext User(short languageId) => new()
    {
        Id = 33001,
        UserKind = Application.Abstractions.Authentication.CurrentUserKind.TenantUser,
        LanguageId = languageId,
        TenantId = 33001,
        OrganizationId = OrganizationId,
        AllowedOrganizationIds = [OrganizationId]
    };

    private async Task SeedContractsAsync()
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

        if (!await context.Languages.AnyAsync(language => language.Id == 1))
            context.Languages.Add(Language(1, "uz", "Uzbek", "O'zbekcha", true, 1));

        if (!await context.Languages.AnyAsync(language => language.Id == 2))
            context.Languages.Add(Language(2, "ru", "Russian", "Русский", false, 2));

        if (!await context.Regions.AnyAsync(region => region.Id == 33001))
        {
            context.Regions.Add(new Region
            {
                Id = 33001,
                ShortName = "Contract test region",
                FullName = "Contract test region",
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == 33001))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = 33001,
                Name = "Contract query tenant",
                Slug = "contract-query-tenant",
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Contract query organization", "330000001"),
                Organization(OtherOrganizationId, "Other contract organization", "330000002"));
        }

        if (!await context.ContractTypes.AnyAsync(contractType => contractType.Id == TranslatedContractTypeId))
        {
            context.ContractTypes.AddRange(
                ContractType(
                    TranslatedContractTypeId,
                    "contract-i18n",
                    "Base translated contract",
                    "Переведённый договор",
                    "Tarjima qilingan shartnoma"),
                ContractType(
                    FallbackContractTypeId,
                    "contract-fallback",
                    "Base fallback contract"));
        }

        if (!await context.CounterpartyCards.IgnoreQueryFilters().AnyAsync(counterparty => counterparty.Id == 33001))
        {
            context.CounterpartyCards.AddRange(
                Counterparty(33001, OrganizationId, "Primary counterparty", "330000011"),
                Counterparty(33002, OtherOrganizationId, "Other counterparty", "330000012"));
        }

        if (!await context.Contracts.IgnoreQueryFilters().AnyAsync(contract => contract.Id == 33001))
        {
            context.Contracts.AddRange(
                Contract(33001, OrganizationId, 33001, TranslatedContractTypeId, "CTR-ALPHA", new DateTime(2026, 9, 3)),
                Contract(33002, OrganizationId, 33001, TranslatedContractTypeId, "CTR-BETA", new DateTime(2026, 9, 2)),
                Contract(33003, OrganizationId, 33001, TranslatedContractTypeId, "CTR-GAMMA", new DateTime(2026, 9, 1)),
                Contract(33004, OrganizationId, 33001, FallbackContractTypeId, "CTR-FALLBACK", new DateTime(2026, 8, 31)),
                Contract(33005, OtherOrganizationId, 33002, TranslatedContractTypeId, "CTR-OTHER", new DateTime(2026, 9, 4)));
        }

        await context.SaveChangesAsync();
    }

    private static Language Language(
        short id,
        string code,
        string name,
        string nativeName,
        bool isDefault,
        short sortOrder) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        NativeName = nativeName,
        IsDefault = isDefault,
        SortOrder = sortOrder,
        StateId = 1,
        CreatedDate = SeedDate
    };

    private static Organization Organization(int id, string name, string inn) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = inn,
        RegionId = 33001,
        IsParent = false,
        StateId = 1,
        CreatedDate = SeedDate,
        TenantId = 33001,
        SetupStatus = "completed"
    };

    private static ContractType ContractType(
        short id,
        string code,
        string baseName,
        string? russianName = null,
        string? uzbekName = null)
    {
        var contractType = new ContractType
        {
            Id = id,
            Code = code,
            Name = baseName,
            StateId = 1,
            CreatedDate = SeedDate
        };

        if (russianName is not null)
        {
            contractType.ContractTypeTranslations.Add(new ContractTypeTranslation
            {
                ContractTypeId = id,
                LanguageId = 2,
                Name = russianName
            });
        }

        if (uzbekName is not null)
        {
            contractType.ContractTypeTranslations.Add(new ContractTypeTranslation
            {
                ContractTypeId = id,
                LanguageId = 1,
                Name = uzbekName
            });
        }

        return contractType;
    }

    private static CounterpartyCard Counterparty(int id, int organizationId, string name, string inn) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        ShortName = name,
        FullName = name,
        Inn = inn,
        StateId = 1,
        CreatedDate = SeedDate
    };

    private static Contract Contract(
        long id,
        int organizationId,
        int counterpartyId,
        short contractTypeId,
        string number,
        DateTime date) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        CounterpartyId = counterpartyId,
        ContractTypeId = contractTypeId,
        ContractNumber = number,
        ContractDate = date,
        StartDate = date,
        StateId = 1,
        CreatedDate = SeedDate
    };
}
