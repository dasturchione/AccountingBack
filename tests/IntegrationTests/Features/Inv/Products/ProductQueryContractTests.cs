using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Products;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace IntegrationTests.Features.Inv.Products;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class ProductQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 49001;
    private const int OrganizationId = 49001;
    private const int OtherOrganizationId = 49002;
    private const int RegionId = 49001;
    private const int TranslatedGroupId = 49011;
    private const int FallbackGroupId = 49012;
    private const short UnitId = 31032;

    [Fact]
    public async Task ListAndDetailStayInsideOrganizationAndUseGroupTranslationWithFallback()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId, LanguageIdConst.RU));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var projection = scope.ServiceProvider
            .GetRequiredService<IProjectionBuilder<Product, ProductListDto>>()
            .Build();

        var page = await service.GetAllAsync(new ProductListFilter
        {
            Search = "scoped product",
            Page = 2,
            PageSize = 1
        });
        var filtered = await service.GetAllAsync(new ProductListFilter
        {
            ProductGroupId = TranslatedGroupId,
            IsPieceTracked = false,
            IsService = false,
            IsSold = true,
            IsPurchased = true,
            Search = "Alpha scoped product",
            Page = 1,
            PageSize = 10
        });
        var translated = await service.GetByIdAsync(49101);
        var fallback = await service.GetByIdAsync(49102);
        var foreign = await service.GetByIdAsync(49103);

        Assert.True(page.IsSuccess);
        Assert.Equal(2, page.Value.TotalCount);
        Assert.Equal("Beta scoped product", Assert.Single(page.Value.Items).Name);
        Assert.True(filtered.IsSuccess);
        Assert.Equal(49101, Assert.Single(filtered.Value.Items).Id);
        Assert.True(translated.IsSuccess);
        Assert.Equal(OrganizationId, translated.Value.OrganizationId);
        Assert.Equal("Переведенная группа товара", translated.Value.ProductGroupName);
        Assert.Equal("Piece", translated.Value.UnitName);
        Assert.Equal("Active", translated.Value.StateName);
        Assert.True(fallback.IsSuccess);
        Assert.Equal("Fallback product group", fallback.Value.ProductGroupName);
        Assert.False(foreign.IsSuccess);
        Assert.Equal("Product.NotFound", foreign.Error.Code);
        var sql = context.Products.Select(projection).ToQueryString();
        Assert.Contains("inv_product_group_translation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke(", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CodeUniquenessIsOrganizationLocalAndForeignProductCannotBeUpdated()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId, LanguageIdConst.RU));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductService>();

        var created = await service.CreateAsync(ProductCreate("CROSS-ORG-PRODUCT-CODE", "Created product"));
        var duplicate = await service.CreateAsync(ProductCreate("OWN-PRODUCT-CODE", "Duplicate product"));
        var foreignUpdate = await service.UpdateAsync(49103, ProductUpdate("FOREIGN-UPDATED", "Foreign updated"));

        Assert.True(created.IsSuccess);
        Assert.False(duplicate.IsSuccess);
        Assert.Equal("Product.CodeConflict", duplicate.Error.Code);
        Assert.False(foreignUpdate.IsSuccess);
        Assert.Equal("Product.NotFound", foreignUpdate.Error.Code);

        await using var context = fixture.CreateDbContext();
        var foreign = await context.Products.IgnoreQueryFilters().SingleAsync(product => product.Id == 49103);
        Assert.Equal(OtherOrganizationId, foreign.OrganizationId);
        Assert.Equal("Other scoped product", foreign.Name);
    }

    [Fact]
    public async Task MissingOrganizationReturnsResultErrorsAndMissingLanguageUsesBaseGroupName()
    {
        await SeedAsync();
        await using var missingProvider = CreateProvider(User(null, LanguageIdConst.RU));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missingService = missingScope.ServiceProvider.GetRequiredService<IProductService>();

        var missingList = await missingService.GetAllAsync(new ProductListFilter());
        var missingDetail = await missingService.GetByIdAsync(49101);

        Assert.False(missingList.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missingList.Error.Code);
        Assert.False(missingDetail.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missingDetail.Error.Code);

        await using var fallbackProvider = CreateProvider(User(OrganizationId, null));
        await using var fallbackScope = fallbackProvider.CreateAsyncScope();
        var fallback = await fallbackScope.ServiceProvider.GetRequiredService<IProductService>().GetByIdAsync(49101);

        Assert.True(fallback.IsSuccess);
        Assert.Equal("Base translated product group", fallback.Value.ProductGroupName);
    }

    private static ProductCreateDto ProductCreate(string code, string name) => new()
    {
        Code = code,
        ProductGroupId = TranslatedGroupId,
        UnitId = UnitId,
        Name = name,
        IsSold = true,
        IsPurchased = true
    };

    private static ProductUpdateDto ProductUpdate(string code, string name) => new()
    {
        Code = code,
        ProductGroupId = TranslatedGroupId,
        UnitId = UnitId,
        Name = name,
        IsSold = true,
        IsPurchased = true,
        StateId = StateIdConst.ACTIVE
    };

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services =>
            {
                services.AddLogging();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<IProductService, ProductService>();
            });

    private static IntegrationTestUserContext User(int? organizationId, short? languageId) => new()
    {
        Id = 49001,
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
        {
            context.States.Add(new State
            {
                Id = StateIdConst.ACTIVE,
                ShortName = "Active",
                FullName = "Active",
                CreatedDate = SeedDate
            });
        }

        if (!await context.Languages.AnyAsync(language => language.Id == LanguageIdConst.RU))
        {
            context.Languages.Add(new Language
            {
                Id = LanguageIdConst.RU,
                Code = "ru",
                Name = "Russian",
                NativeName = "Русский",
                SortOrder = 2,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Regions.AnyAsync(region => region.Id == RegionId))
        {
            context.Regions.Add(new Region
            {
                Id = RegionId,
                ShortName = "Product region",
                FullName = "Product region",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Units.AnyAsync(unit => unit.Id == UnitId))
        {
            context.Units.Add(new Unit
            {
                Id = UnitId,
                Code = "PCS-PR",
                Name = "Piece",
                StateId = StateIdConst.ACTIVE
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = TenantId,
                Name = "Product tenant",
                Slug = "product-contract-tenant",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Selected product organization", "490000001"),
                Organization(OtherOrganizationId, "Other product organization", "490000002"));
        }

        if (!await context.ProductGroups.AnyAsync(group => group.Id == TranslatedGroupId))
        {
            var translatedGroup = Group(TranslatedGroupId, "INV-PRODUCT-GROUP", "Base translated product group");
            translatedGroup.ProductGroupTranslations.Add(new ProductGroupTranslation
            {
                ProductGroupId = TranslatedGroupId,
                LanguageId = LanguageIdConst.RU,
                Name = "Переведенная группа товара"
            });
            context.ProductGroups.AddRange(
                translatedGroup,
                Group(FallbackGroupId, "INV-PRODUCT-FALLBACK", "Fallback product group"));
        }

        if (!await context.Products.IgnoreQueryFilters().AnyAsync(product => product.Id == 49101))
        {
            context.Products.AddRange(
                Product(49101, OrganizationId, TranslatedGroupId, "Alpha scoped product", "OWN-PRODUCT-CODE"),
                Product(49102, OrganizationId, FallbackGroupId, "Beta scoped product", "OWN-PRODUCT-CODE-B"),
                Product(49103, OtherOrganizationId, TranslatedGroupId, "Other scoped product", "CROSS-ORG-PRODUCT-CODE"));
        }

        await context.Products.IgnoreQueryFilters()
            .Where(product => product.OrganizationId == OrganizationId && product.Code == "CROSS-ORG-PRODUCT-CODE")
            .ExecuteDeleteAsync();

        await context.SaveChangesAsync();
    }

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

    private static ProductGroup Group(int id, string code, string name) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        IsAssignable = true,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static Product Product(int id, int organizationId, int groupId, string name, string code) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        ProductGroupId = groupId,
        UnitId = UnitId,
        Name = name,
        Code = code,
        IsSold = true,
        IsPurchased = true,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };
}
