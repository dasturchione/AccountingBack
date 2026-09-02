using Application.Abstractions.Authentication;
using Application.Features.Inv.ProductPrices;
using Domain.Entities;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace IntegrationTests.Features.Inv.ProductPrices;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class ProductPriceQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 53001;
    private const int OrganizationId = 53001;
    private const int OtherOrganizationId = 53002;
    private const int RegionId = 53001;
    private const int ProductId = 53001;
    private const int SecondProductId = 53002;
    private const int OtherProductId = 53003;
    private const short CurrencyId = 30001;
    private const short UnitId = 30001;
    private const short VatRateId = 30001;

    [Fact]
    public async Task ListAndDetailUseSelectedOrganizationTranslationAndSqlPaging()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductPriceService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var projection = scope.ServiceProvider
            .GetRequiredService<IProjectionBuilder<ProductPrice, ProductPriceListDto>>()
            .Build();

        var sql = context.ProductPrices.Select(projection).ToQueryString();
        var page = await service.GetAllAsync(new ProductPriceListFilter
        {
            PriceTypeId = PriceTypeIdConst.AVERAGE_COST_PRICE,
            Search = "priced product",
            Page = 2,
            PageSize = 1
        });
        var detail = await service.GetByIdAsync(53001);
        var foreign = await service.GetByIdAsync(53004);

        Assert.True(page.IsSuccess);
        Assert.Equal(3, page.Value.TotalCount);
        Assert.Equal(53001, Assert.Single(page.Value.Items).Id);
        Assert.DoesNotContain(page.Value.Items, item => item.OrganizationId != OrganizationId);
        Assert.True(detail.IsSuccess);
        Assert.Equal("Тестовая валюта цен", detail.Value.CurrencyName);
        Assert.Contains("cmn_currency_translation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.False(foreign.IsSuccess);
        Assert.Equal("ProductPrice.NotFound", foreign.Error.Code);
    }

    [Fact]
    public async Task DetailsUseMostRecentEffectiveOrganizationPricesAndConditions()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();

        var result = await scope.ServiceProvider
            .GetRequiredService<IProductPriceService>()
            .GetPriceDetailsByProductIdAsync(ProductId);

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, result.Value.Cost.CostPrice);
        Assert.Equal(110m, result.Value.Sale.SalePrice);
        Assert.Empty(result.Value.Cost.Purchases);
        Assert.Empty(result.Value.Sale.SalePrices);
    }

    [Fact]
    public async Task MutationsRejectForeignProductsPricesAndMissingOrganization()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductPriceService>();

        var created = await service.CreateAsync(CreateDto(ProductId, 321m));
        var foreignProduct = await service.CreateAsync(CreateDto(OtherProductId, 321m));
        var foreignPrice = await service.UpdateAsync(53004, UpdateDto(ProductId, 321m));
        var foreignProductUpdate = await service.UpdateAsync(53001, UpdateDto(OtherProductId, 321m));

        Assert.True(created.IsSuccess);
        Assert.False(foreignProduct.IsSuccess);
        Assert.Equal("ProductPrice.ProductNotFound", foreignProduct.Error.Code);
        Assert.False(foreignPrice.IsSuccess);
        Assert.Equal("ProductPrice.NotFound", foreignPrice.Error.Code);
        Assert.False(foreignProductUpdate.IsSuccess);
        Assert.Equal("ProductPrice.ProductNotFound", foreignProductUpdate.Error.Code);

        await using var missingProvider = CreateProvider(User(null));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missing = await missingScope.ServiceProvider
            .GetRequiredService<IProductPriceService>()
            .GetAllAsync(new ProductPriceListFilter());

        Assert.False(missing.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missing.Error.Code);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services =>
            {
                services.AddScoped<IProductPriceCalculateService, ProductPriceCalculateService>();
                services.AddScoped<IProductPriceService, ProductPriceService>();
            });

    private static IntegrationTestUserContext User(int? organizationId) => new()
    {
        Id = 53001,
        UserKind = CurrentUserKind.SuperAdmin,
        LanguageId = LanguageIdConst.RU,
        TenantId = TenantId,
        OrganizationId = organizationId,
        AllowedOrganizationIds = organizationId.HasValue ? [organizationId.Value] : []
    };

    private static ProductPriceCreateDto CreateDto(int productId, decimal price) => new()
    {
        ProductId = productId,
        CurrencyId = CurrencyId,
        PriceTypeId = PriceTypeIdConst.FIXED_SALE_PRICE,
        UnitId = UnitId,
        Price = price,
        StartDate = new DateTime(2026, 1, 1)
    };

    private static ProductPriceUpdateDto UpdateDto(int productId, decimal price) => new()
    {
        ProductId = productId,
        CurrencyId = CurrencyId,
        PriceTypeId = PriceTypeIdConst.AVERAGE_COST_PRICE,
        UnitId = UnitId,
        Price = price,
        StartDate = new DateTime(2026, 1, 1),
        StateId = StateIdConst.ACTIVE
    };

    private async Task SeedAsync()
    {
        await using var context = fixture.CreateDbContext();
        await SeedReferencesAsync(context);

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(x => x.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Price organization", "530000001"),
                Organization(OtherOrganizationId, "Other price organization", "530000002"));
        }

        if (!await context.Products.IgnoreQueryFilters().AnyAsync(x => x.Id == ProductId))
        {
            context.Products.AddRange(
                Product(ProductId, OrganizationId, "Alpha priced product"),
                Product(SecondProductId, OrganizationId, "Beta priced product"),
                Product(OtherProductId, OtherOrganizationId, "Other priced product"));
        }

        if (!await context.ProductPrices.IgnoreQueryFilters().AnyAsync(x => x.Id == 53001))
        {
            context.ProductPrices.AddRange(
                Price(53001, OrganizationId, ProductId, PriceTypeIdConst.AVERAGE_COST_PRICE, 100m, new DateTime(2026, 1, 1)),
                Price(53002, OrganizationId, ProductId, PriceTypeIdConst.FIXED_SALE_PRICE, 250m, new DateTime(2026, 1, 1)),
                Price(53003, OrganizationId, SecondProductId, PriceTypeIdConst.AVERAGE_COST_PRICE, 200m, new DateTime(2026, 3, 1)),
                Price(53004, OtherOrganizationId, OtherProductId, PriceTypeIdConst.AVERAGE_COST_PRICE, 999m, new DateTime(2026, 8, 1)),
                Price(53005, OrganizationId, ProductId, PriceTypeIdConst.AVERAGE_COST_PRICE, 80m, new DateTime(2025, 1, 1)));
        }

        if (!await context.PricingConditions.IgnoreQueryFilters().AnyAsync(x => x.Id == 53001))
        {
            context.PricingConditions.AddRange(
                PricingCondition(53001, OrganizationId, PricingMethodIdConst.FIXED_PRICE, 0m, new DateTime(2025, 1, 1)),
                PricingCondition(53002, OrganizationId, PricingMethodIdConst.COST_PLUS_PERCENT, 10m, new DateTime(2026, 1, 1)),
                PricingCondition(53003, OtherOrganizationId, PricingMethodIdConst.COST_PLUS_AMOUNT, 500m, new DateTime(2026, 8, 1)));
        }

        if (!await context.SaleConditions.IgnoreQueryFilters().AnyAsync(x => x.Id == 53001))
        {
            context.SaleConditions.Add(new SaleCondition
            {
                Id = 53001,
                OrganizationId = OrganizationId,
                CostingMethodId = CostingMethodIdConst.AVERAGE,
                VatRateId = VatRateId,
                StartDate = new DateTime(2026, 1, 1),
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        await context.ProductPrices.IgnoreQueryFilters()
            .Where(x => x.OrganizationId == OrganizationId &&
                        x.Id != 53001 && x.Id != 53002 && x.Id != 53003 && x.Id != 53005)
            .ExecuteDeleteAsync();
        await context.SaveChangesAsync();
    }

    private static async Task SeedReferencesAsync(AppDbContext context)
    {
        if (!await context.States.AnyAsync(x => x.Id == StateIdConst.ACTIVE))
            context.States.Add(new State { Id = StateIdConst.ACTIVE, ShortName = "Active", FullName = "Active", CreatedDate = SeedDate });
        if (!await context.Languages.AnyAsync(x => x.Id == LanguageIdConst.RU))
            context.Languages.Add(new Language { Id = LanguageIdConst.RU, Code = "ru-price", Name = "Russian", NativeName = "Русский", IsDefault = false, SortOrder = 2, StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate });
        if (!await context.Regions.AnyAsync(x => x.Id == RegionId))
            context.Regions.Add(new Region { Id = RegionId, ShortName = "Price region", FullName = "Price region", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate });
        if (!await context.PlatformTenants.AnyAsync(x => x.Id == TenantId))
            context.PlatformTenants.Add(new PlatformTenant { Id = TenantId, Name = "Price tenant", Slug = "product-price-contract-tenant", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate });
        if (!await context.Units.AnyAsync(x => x.Id == UnitId))
            context.Units.Add(new Unit { Id = UnitId, Code = "PRICE-UNIT", Name = "Price unit", StateId = StateIdConst.ACTIVE });
        if (!await context.Currencies.AnyAsync(x => x.Id == CurrencyId))
        {
            context.Currencies.Add(new Currency { Id = CurrencyId, Code = "TPC", Name = "Price currency", StateId = StateIdConst.ACTIVE });
            context.CurrencyTranslations.Add(new CurrencyTranslation { CurrencyId = CurrencyId, LanguageId = LanguageIdConst.RU, Name = "Тестовая валюта цен" });
        }
        if (!await context.ProductPriceTypes.AnyAsync(x => x.Id == PriceTypeIdConst.AVERAGE_COST_PRICE))
            context.ProductPriceTypes.Add(new ProductPriceType { Id = PriceTypeIdConst.AVERAGE_COST_PRICE, Code = "AVERAGE-COST", Name = "Average cost" });
        if (!await context.ProductPriceTypes.AnyAsync(x => x.Id == PriceTypeIdConst.FIXED_SALE_PRICE))
            context.ProductPriceTypes.Add(new ProductPriceType { Id = PriceTypeIdConst.FIXED_SALE_PRICE, Code = "FIXED-SALE", Name = "Fixed sale" });
        if (!await context.PricingMethods.AnyAsync(x => x.Id == PricingMethodIdConst.COST_PLUS_PERCENT))
            context.PricingMethods.Add(new PricingMethod { Id = PricingMethodIdConst.COST_PLUS_PERCENT, Code = "COST-PERCENT", Name = "Cost plus percent" });
        if (!await context.PricingMethods.AnyAsync(x => x.Id == PricingMethodIdConst.COST_PLUS_AMOUNT))
            context.PricingMethods.Add(new PricingMethod { Id = PricingMethodIdConst.COST_PLUS_AMOUNT, Code = "COST-AMOUNT", Name = "Cost plus amount" });
        if (!await context.PricingMethods.AnyAsync(x => x.Id == PricingMethodIdConst.FIXED_PRICE))
            context.PricingMethods.Add(new PricingMethod { Id = PricingMethodIdConst.FIXED_PRICE, Code = "FIXED-PRICE", Name = "Fixed price" });
        if (!await context.PriceRoundingMethods.AnyAsync(x => x.Id == PriceRoundingMethodIdConst.NONE))
            context.PriceRoundingMethods.Add(new PriceRoundingMethod { Id = PriceRoundingMethodIdConst.NONE, Code = "NONE", Name = "No rounding" });
        if (!await context.CostingMethods.AnyAsync(x => x.Id == CostingMethodIdConst.AVERAGE))
            context.CostingMethods.Add(new CostingMethod { Id = CostingMethodIdConst.AVERAGE, Code = "AVERAGE", Name = "Average" });
        if (!await context.VatRates.AnyAsync(x => x.Id == VatRateId))
            context.VatRates.Add(new VatRate { Id = VatRateId, Code = "PRICE-VAT", Name = "Price VAT", Rate = 12m, StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate });
    }

    private static Organization Organization(int id, string name, string inn) => new()
    {
        Id = id, ShortName = name, FullName = name, Inn = inn, RegionId = RegionId,
        IsParent = false, StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate,
        TenantId = TenantId, SetupStatus = "completed"
    };

    private static Product Product(int id, int organizationId, string name) => new()
    {
        Id = id, OrganizationId = organizationId, UnitId = UnitId, Name = name,
        IsService = false, IsPieceTracked = false, IsSold = true, IsPurchased = true,
        StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate
    };

    private static ProductPrice Price(long id, int organizationId, int productId, short typeId, decimal price, DateTime startDate) => new()
    {
        Id = id, OrganizationId = organizationId, ProductId = productId, CurrencyId = CurrencyId,
        PriceTypeId = typeId, UnitId = UnitId, Price = price, StartDate = startDate,
        StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate
    };

    private static PricingCondition PricingCondition(long id, int organizationId, short methodId, decimal value, DateTime startDate) => new()
    {
        Id = id, OrganizationId = organizationId, PricingMethodId = methodId,
        PricingValue = value, RoundingMethodId = PriceRoundingMethodIdConst.NONE,
        RoundingPrecision = 1m, StartDate = startDate, StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };
}
