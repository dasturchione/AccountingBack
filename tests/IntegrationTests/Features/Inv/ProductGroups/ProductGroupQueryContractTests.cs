using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.ProductGroups;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace IntegrationTests.Features.Inv.ProductGroups;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class ProductGroupQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 48001;
    private const int OrganizationId = 48001;
    private const int OtherOrganizationId = 48002;
    private const int RegionId = 48001;
    private const short UnitId = 31031;

    [Fact]
    public async Task ListUsesTranslationSearchStablePagingAndOrganizationScopedServiceFilter()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId, LanguageIdConst.RU));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductGroupService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var projection = scope.ServiceProvider
            .GetRequiredService<IProjectionBuilder<ProductGroup, ProductGroupListDto>>()
            .Build();

        var sql = context.ProductGroups.Select(projection).ToQueryString();
        var page = await service.GetAllAsync(new ProductGroupListFilter
        {
            Search = "Переведенная группа",
            Page = 2,
            PageSize = 1
        });
        var serviceGroups = await service.GetAllAsync(new ProductGroupListFilter
        {
            IsService = true,
            Search = "Переведенная группа",
            Page = 1,
            PageSize = 10
        });

        Assert.True(page.IsSuccess);
        Assert.Equal(3, page.Value.TotalCount);
        Assert.Equal("Переведенная группа Бета", Assert.Single(page.Value.Items).Name);
        Assert.True(serviceGroups.IsSuccess);
        Assert.Equal(
            ["Переведенная группа Бета", "Переведенная группа Гамма"],
            serviceGroups.Value.Items.Select(group => group.Name));
        Assert.Contains("inv_product_group_translation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke(", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DetailScopesNestedProductsAndUsesBaseFallback()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId, LanguageIdConst.RU));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductGroupService>();

        var translated = await service.GetByIdAsync(48011);
        var serviceOnly = await service.GetByIdAsync(48011, isService: true);
        var fallback = await service.GetByIdAsync(48014);

        Assert.True(translated.IsSuccess);
        Assert.Equal("Переведенная группа Альфа", translated.Value.Name);
        Assert.Equal("Own non-service product", Assert.Single(translated.Value.Products).Name);
        Assert.Equal(OrganizationId, translated.Value.Products[0].OrganizationId);
        Assert.Equal("Piece", translated.Value.Products[0].UnitName);
        Assert.True(serviceOnly.IsSuccess);
        Assert.Empty(serviceOnly.Value.Products);
        Assert.True(fallback.IsSuccess);
        Assert.Equal("Fallback product group", fallback.Value.Name);
    }

    [Fact]
    public async Task DuplicateCodeReturnsLocalizedConflictAndDetailRequiresOrganizationContext()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId, LanguageIdConst.RU));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductGroupService>();

        var duplicate = await service.CreateAsync(new ProductGroupCreateDto
        {
            Code = "INV-PG-ALPHA",
            Name = "Duplicate group"
        });

        Assert.False(duplicate.IsSuccess);
        Assert.Equal("ProductGroup.CodeConflict", duplicate.Error.Code);

        await using var missingProvider = CreateProvider(User(null, LanguageIdConst.RU));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missingContext = await missingScope.ServiceProvider
            .GetRequiredService<IProductGroupService>()
            .GetByIdAsync(48011);

        Assert.False(missingContext.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missingContext.Error.Code);
    }

    [Fact]
    public async Task MissingLanguageUsesBaseNameInsteadOfAnArbitraryLanguage()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId, null));
        await using var scope = provider.CreateAsyncScope();

        var result = await scope.ServiceProvider
            .GetRequiredService<IProductGroupService>()
            .GetByIdAsync(48011);

        Assert.True(result.IsSuccess);
        Assert.Equal("Base product group alpha", result.Value.Name);
    }

    [Fact]
    public async Task UpdatingGroupProductsDoesNotDeactivateAnotherOrganizationsProducts()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId, LanguageIdConst.RU));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductGroupService>();

        var result = await service.UpdateAsync(48011, new ProductGroupUpdateDto
        {
            Code = "INV-PG-ALPHA",
            Name = "Base product group alpha",
            IsAssignable = true,
            SortOrder = 10,
            StateId = StateIdConst.ACTIVE,
            Products =
            [
                new ProductInGroupUpdateDto
                {
                    Id = 48101,
                    UnitId = UnitId,
                    Name = "Own non-service product",
                    IsSold = true,
                    IsPurchased = true,
                    StateId = StateIdConst.ACTIVE
                }
            ]
        });

        Assert.True(result.IsSuccess);
        await using var context = fixture.CreateDbContext();
        var foreignProduct = await context.Products.IgnoreQueryFilters().SingleAsync(product => product.Id == 48102);
        Assert.Equal(StateIdConst.ACTIVE, foreignProduct.StateId);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services =>
            {
                services.AddLogging();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<IProductGroupService, ProductGroupService>();
            });

    private static IntegrationTestUserContext User(int? organizationId, short? languageId) => new()
    {
        Id = 48001,
        UserKind = CurrentUserKind.SuperAdmin,
        LanguageId = languageId,
        TenantId = TenantId,
        OrganizationId = organizationId,
        AllowedOrganizationIds = organizationId.HasValue ? [organizationId.Value] : []
    };

    private async Task SeedAsync()
    {
        await using var context = fixture.CreateDbContext();

        if (!await context.States.AnyAsync(state => state.Id == StateIdConst.ACTIVE))
            context.States.Add(State());

        if (!await context.Languages.AnyAsync(language => language.Id == LanguageIdConst.RU))
            context.Languages.Add(Language());

        if (!await context.Regions.AnyAsync(region => region.Id == RegionId))
            context.Regions.Add(Region());

        if (!await context.Units.AnyAsync(unit => unit.Id == UnitId))
        {
            context.Units.Add(new Unit
            {
                Id = UnitId,
                Code = "PCS-PG",
                Name = "Piece",
                StateId = StateIdConst.ACTIVE
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
            context.PlatformTenants.Add(Tenant());

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Selected product group organization", "480000001"),
                Organization(OtherOrganizationId, "Other product group organization", "480000002"));
        }

        if (!await context.ProductGroups.AnyAsync(group => group.Id == 48011))
        {
            context.ProductGroups.AddRange(
                Group(48011, "INV-PG-ALPHA", "Base product group alpha", "Переведенная группа Альфа"),
                Group(48012, "INV-PG-BETA", "Base product group beta", "Переведенная группа Бета"),
                Group(48013, "INV-PG-GAMMA", "Base product group gamma", "Переведенная группа Гамма"),
                Group(48014, "INV-PG-FALLBACK", "Fallback product group"));
        }

        if (!await context.Products.IgnoreQueryFilters().AnyAsync(product => product.Id == 48101))
        {
            context.Products.AddRange(
                Product(48101, OrganizationId, 48011, "Own non-service product", false),
                Product(48102, OtherOrganizationId, 48011, "Foreign service product", true),
                Product(48103, OrganizationId, 48012, "Own service product beta", true),
                Product(48104, OrganizationId, 48013, "Own service product gamma", true));
        }

        await context.SaveChangesAsync();
    }

    private static State State() => new()
    {
        Id = StateIdConst.ACTIVE,
        ShortName = "Active",
        FullName = "Active",
        CreatedDate = SeedDate
    };

    private static Language Language() => new()
    {
        Id = LanguageIdConst.RU,
        Code = "ru",
        Name = "Russian",
        NativeName = "Русский",
        SortOrder = 2,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static Region Region() => new()
    {
        Id = RegionId,
        ShortName = "Product group region",
        FullName = "Product group region",
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static PlatformTenant Tenant() => new()
    {
        Id = TenantId,
        Name = "Product group tenant",
        Slug = "product-group-contract-tenant",
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static Organization Organization(int id, string name, string inn) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = inn,
        RegionId = RegionId,
        IsParent = false,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate,
        TenantId = TenantId,
        SetupStatus = "completed"
    };

    private static ProductGroup Group(int id, string code, string baseName, string? russianName = null)
    {
        var group = new ProductGroup
        {
            Id = id,
            Code = code,
            Name = baseName,
            IsAssignable = true,
            SortOrder = 10,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = SeedDate
        };

        if (russianName is not null)
        {
            group.ProductGroupTranslations.Add(new ProductGroupTranslation
            {
                ProductGroupId = id,
                LanguageId = LanguageIdConst.RU,
                Name = russianName
            });
        }

        return group;
    }

    private static Product Product(int id, int organizationId, int groupId, string name, bool isService) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        ProductGroupId = groupId,
        UnitId = UnitId,
        Name = name,
        IsService = isService,
        IsSold = true,
        IsPurchased = true,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };
}
