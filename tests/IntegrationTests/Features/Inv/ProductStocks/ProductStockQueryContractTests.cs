using Application.Abstractions.Authentication;
using Application.Features.Inv.ProductPrices;
using Application.Features.Inv.ProductStocks;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;
using SharedKernel.Results;
using AppDbContext = global::Infrastructure.Persistence.AppDbContext;

namespace IntegrationTests.Features.Inv.ProductStocks;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class ProductStockQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 54001;
    private const int OrganizationId = 54001;
    private const int OtherOrganizationId = 54002;
    private const int RegionId = 54001;
    private const int WarehouseId = 54001;
    private const int SecondWarehouseId = 54002;
    private const int OtherWarehouseId = 54003;
    private const int ProductId = 54001;
    private const int SecondProductId = 54002;
    private const int OtherProductId = 54003;
    private const int ProductGroupId = 54001;
    private const int SecondProductGroupId = 54002;
    private const short UnitId = 30002;

    [Fact]
    public async Task ProductSummaryAggregatesWarehousesAndUsesRequestedTranslations()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();

        var result = await scope.ServiceProvider
            .GetRequiredService<IProductStockService>()
            .GetProductsStockAsync(new ProductStockFilter
            {
                Search = "Товар А",
                Page = 1,
                PageSize = 10
            });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
        var product = Assert.Single(result.Value.Items);
        Assert.Equal(ProductId, product.ProductId);
        Assert.Equal("Товар А", product.ProductName);
        Assert.Equal("Группа А", product.ProductGroupName);
        Assert.Equal("штука", product.UnitName);
        Assert.Equal(6m, product.Quantity);
        Assert.Equal(1m, product.ReservedQuantity);
        Assert.Equal(1m, product.BlockedQuantity);
        Assert.Equal(4m, product.AvailableQuantity);
        Assert.Equal(2, product.MarkingCount);
        Assert.Equal([54001, 54002], product.AvailableProductTableIds);
        Assert.Equal(2, product.Batches.Count);
        Assert.Equal(4m, product.Batches.Sum(batch => batch.AvailableQuantity));
    }

    [Fact]
    public async Task GroupSummaryUsesAggregatedQuantitiesAndPriceMaps()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();

        var result = await scope.ServiceProvider
            .GetRequiredService<IProductStockService>()
            .GetProductGroupsStockAsync(new ProductGroupStockFilter { Page = 1, PageSize = 10 });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        var first = result.Value.Items.First();
        Assert.Equal(ProductGroupId, first.Id);
        Assert.Equal("Группа А", first.Name);
        Assert.Equal(6m, first.Quantity);
        Assert.Equal(10m, first.Price);
        Assert.Equal(4m, first.CostPrice);
        Assert.Equal(60m, first.TotalAmount);
    }

    [Fact]
    public async Task MarkingAndTableQueriesStayInsideOrganizationAndPageDeterministically()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductStockService>();

        var marking = await service.GetByMarkingNumberAsync("MARK-A1");
        var foreignMarking = await service.GetByMarkingNumberAsync("MARK-OTHER");
        var tables = await service.GetProductTablesStockAsync(new ProductTableStockFilter
        {
            WarehouseId = WarehouseId,
            Page = 1,
            PageSize = 1
        });

        Assert.True(marking.IsSuccess);
        Assert.Equal(ProductId, marking.Value.ProductId);
        Assert.Equal("Товар А", marking.Value.ProductName);
        Assert.Equal("Основной склад", marking.Value.CurrentWarehouseName);
        Assert.False(foreignMarking.IsSuccess);
        Assert.Equal("ProductTable.NotFoundByMarkingNumber", foreignMarking.Error.Code);
        Assert.True(tables.IsSuccess);
        Assert.Equal(2, tables.Value.TotalCount);
        Assert.Equal(54001, Assert.Single(tables.Value.Items).Id);
        Assert.Equal("Товар А", tables.Value.Items.First().ProductName);
    }

    [Fact]
    public async Task WarehouseFilterAndMissingOrganizationReturnLocalizedErrors()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();

        var foreignWarehouse = await scope.ServiceProvider
            .GetRequiredService<IWarehouseInventoryService>()
            .GetWarehouseProductsAsync(new WarehouseProductFilter { WarehouseId = OtherWarehouseId });

        Assert.False(foreignWarehouse.IsSuccess);
        Assert.Equal("Warehouse.NotFound", foreignWarehouse.Error.Code);

        await using var missingProvider = CreateProvider(User(null));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missing = await missingScope.ServiceProvider
            .GetRequiredService<IProductStockService>()
            .GetProductTablesStockAsync(new ProductTableStockFilter());

        Assert.False(missing.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missing.Error.Code);
    }

    [Fact]
    public async Task CurrentAndHistoricalCalculationsAllowAllProductGroups()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductStockCalculateService>();

        var current = await service.GetProductsAsync(
            OrganizationId,
            WarehouseId,
            productGroupId: null,
            productIds: [ProductId]);
        var historical = await service.GetProductsAsync(
            OrganizationId,
            WarehouseId,
            productGroupId: null,
            choosedDate: new DateOnly(2026, 8, 25),
            productIds: [ProductId]);

        Assert.True(current.IsSuccess);
        Assert.Equal((3m, 2m, 1m, 0m), current.Value[ProductId]);
        Assert.True(historical.IsSuccess);
        Assert.Equal((3m, 3m, 0m, 0m), historical.Value[ProductId]);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services =>
            {
                services.AddScoped<IProductPriceCalculateService, StubProductPriceCalculateService>();
                services.AddScoped<IWarehouseInventoryService, WarehouseInventoryService>();
                services.AddScoped<IProductStockService, ProductStockService>();
                services.AddScoped<IProductStockCalculateService, ProductStockCalculateService>();
            });

    private static IntegrationTestUserContext User(int? organizationId) => new()
    {
        Id = 54001,
        UserKind = CurrentUserKind.SuperAdmin,
        LanguageId = LanguageIdConst.RU,
        TenantId = TenantId,
        OrganizationId = organizationId,
        AllowedOrganizationIds = organizationId.HasValue ? [organizationId.Value] : []
    };

    private async Task SeedAsync()
    {
        await using var context = fixture.CreateDbContext();
        await SeedReferencesAsync(context);

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(x => x.Id == OrganizationId))
            context.Organizations.AddRange(
                Organization(OrganizationId, "Stock organization", "540000001"),
                Organization(OtherOrganizationId, "Other stock organization", "540000002"));

        if (!await context.ProductGroups.AnyAsync(x => x.Id == ProductGroupId))
            context.ProductGroups.AddRange(
                ProductGroup(ProductGroupId, "Base group A"),
                ProductGroup(SecondProductGroupId, "Base group B"));

        if (!await context.Products.IgnoreQueryFilters().AnyAsync(x => x.Id == ProductId))
            context.Products.AddRange(
                Product(ProductId, OrganizationId, ProductGroupId, "Base product A", true),
                Product(SecondProductId, OrganizationId, SecondProductGroupId, "Base product B", false),
                Product(OtherProductId, OtherOrganizationId, ProductGroupId, "Other product", true));

        if (!await context.Warehouses.IgnoreQueryFilters().AnyAsync(x => x.Id == WarehouseId))
            context.Warehouses.AddRange(
                Warehouse(WarehouseId, OrganizationId, "Base main warehouse"),
                Warehouse(SecondWarehouseId, OrganizationId, "Base second warehouse"),
                Warehouse(OtherWarehouseId, OtherOrganizationId, "Other warehouse"));

        if (!await context.WarehouseProducts.IgnoreQueryFilters().AnyAsync(x => x.WarehouseId == WarehouseId && x.ProductId == ProductId))
            context.WarehouseProducts.AddRange(
                WarehouseProduct(WarehouseId, ProductId, 3m, 1m, 0m),
                WarehouseProduct(SecondWarehouseId, ProductId, 3m, 0m, 1m),
                WarehouseProduct(WarehouseId, SecondProductId, 4m, 0m, 0m),
                WarehouseProduct(OtherWarehouseId, OtherProductId, 7m, 0m, 0m));

        if (!await context.ProductTables.AnyAsync(x => x.Id == 54001))
        {
            context.ProductTables.AddRange(
                ProductTable(54001, ProductId, "SER-002", "MARK-A1"),
                ProductTable(54002, ProductId, "SER-001", "MARK-A2"),
                ProductTable(54003, ProductId, "SER-003", "MARK-RESERVED"),
                ProductTable(54004, SecondProductId, "SER-B", "MARK-B"),
                ProductTable(54005, OtherProductId, "SER-O", "MARK-OTHER"));
            context.WarehouseProductTables.AddRange(
                WarehouseProductTable(WarehouseId, 54001, ProductTableStatusIdConst.IN_STOCK),
                WarehouseProductTable(SecondWarehouseId, 54002, ProductTableStatusIdConst.IN_STOCK),
                WarehouseProductTable(WarehouseId, 54003, ProductTableStatusIdConst.RESERVED),
                WarehouseProductTable(WarehouseId, 54004, ProductTableStatusIdConst.IN_STOCK),
                WarehouseProductTable(OtherWarehouseId, 54005, ProductTableStatusIdConst.IN_STOCK));
        }

        if (!await context.WarehouseProductMovements.IgnoreQueryFilters().AnyAsync(x => x.Id == 54001))
        {
            context.WarehouseProductMovements.AddRange(
                Movement(54001, OrganizationId, WarehouseId, ProductId, 9001, 5m, MovementDirectionIdConst.IN, new DateTime(2026, 8, 20)),
                Movement(54002, OrganizationId, WarehouseId, ProductId, 9002, 2m, MovementDirectionIdConst.OUT, new DateTime(2026, 8, 22)),
                Movement(54003, OrganizationId, SecondWarehouseId, ProductId, 9003, 3m, MovementDirectionIdConst.IN, new DateTime(2026, 8, 21)),
                Movement(54004, OtherOrganizationId, OtherWarehouseId, OtherProductId, 9004, 7m, MovementDirectionIdConst.IN, new DateTime(2026, 8, 20)));
            context.WarehouseProductBatches.AddRange(
                Batch(54001, OrganizationId, WarehouseId, ProductId, 54001, 3m, 4m, "A-1", new DateTime(2026, 8, 20)),
                Batch(54002, OrganizationId, SecondWarehouseId, ProductId, 54003, 3m, 4m, "A-2", new DateTime(2026, 8, 21)));
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedReferencesAsync(AppDbContext context)
    {
        if (!await context.States.AnyAsync(x => x.Id == StateIdConst.ACTIVE))
            context.States.Add(new State { Id = StateIdConst.ACTIVE, ShortName = "Active", FullName = "Active", CreatedDate = SeedDate });
        if (!await context.Languages.AnyAsync(x => x.Id == LanguageIdConst.RU))
            context.Languages.Add(new Language { Id = LanguageIdConst.RU, Code = "ru-stock", Name = "Russian", NativeName = "Русский", SortOrder = 2, StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate });
        if (!await context.Regions.AnyAsync(x => x.Id == RegionId))
            context.Regions.Add(new Region { Id = RegionId, ShortName = "Stock region", FullName = "Stock region", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate });
        if (!await context.PlatformTenants.AnyAsync(x => x.Id == TenantId))
            context.PlatformTenants.Add(new PlatformTenant { Id = TenantId, Name = "Stock tenant", Slug = "product-stock-contract-tenant", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate });
        if (!await context.Units.AnyAsync(x => x.Id == UnitId))
            context.Units.Add(new Unit { Id = UnitId, Code = "STOCK-UNIT", Name = "Base unit", StateId = StateIdConst.ACTIVE });
        foreach (var statusId in new[] { ProductTableStatusIdConst.IN_STOCK, ProductTableStatusIdConst.RESERVED })
            if (!await context.ProductTableStatuses.AnyAsync(x => x.Id == statusId))
                context.ProductTableStatuses.Add(new ProductTableStatus { Id = statusId, Code = $"STATUS-{statusId}", Name = $"Status {statusId}", StateId = StateIdConst.ACTIVE });
        foreach (var directionId in new[] { MovementDirectionIdConst.OUT, MovementDirectionIdConst.IN })
            if (!await context.MovementDirections.AnyAsync(x => x.Id == directionId))
                context.MovementDirections.Add(new MovementDirection { Id = directionId, Code = directionId < 0 ? "OUT" : "IN", Name = directionId < 0 ? "Out" : "In" });
        if (!await context.DocumentTypes.AnyAsync(x => x.Id == DocumentTypeIdConst.PURCHASE))
            context.DocumentTypes.Add(new DocumentType { Id = DocumentTypeIdConst.PURCHASE, Code = "PURCHASE", Name = "Purchase", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate });
        await AddTranslationAsync(context, 54101, "inv_product", ProductId, "Товар А");
        await AddTranslationAsync(context, 54102, "inv_product", SecondProductId, "Товар Б");
        await AddTranslationAsync(context, 54103, "inv_product_group", ProductGroupId, "Группа А");
        await AddTranslationAsync(context, 54104, "inv_product_group", SecondProductGroupId, "Группа Б");
        await AddTranslationAsync(context, 54105, "cmn_unit", UnitId, "штука");
        await AddTranslationAsync(context, 54106, "inv_warehouse", WarehouseId, "Основной склад");
    }

    private static async Task AddTranslationAsync(AppDbContext context, long id, string table, long recordId, string value)
    {
        if (!await context.Translations.AnyAsync(x => x.LanguageId == LanguageIdConst.RU && x.TableName == table && x.RecordId == recordId && x.ColumnName == "name"))
            context.Translations.Add(new Translation { Id = id, LanguageId = LanguageIdConst.RU, TableName = table, RecordId = recordId, ColumnName = "name", Value = value, CreatedDate = SeedDate });
    }

    private static Organization Organization(int id, string name, string inn) => new()
    {
        Id = id, ShortName = name, FullName = name, Inn = inn, RegionId = RegionId,
        IsParent = false, StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate,
        TenantId = TenantId, SetupStatus = "completed"
    };

    private static ProductGroup ProductGroup(int id, string name) => new()
    {
        Id = id, Code = $"STOCK-GROUP-{id}", Name = name, IsAssignable = true,
        SortOrder = id, StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate
    };

    private static Product Product(int id, int organizationId, int groupId, string name, bool pieceTracked) => new()
    {
        Id = id, OrganizationId = organizationId, ProductGroupId = groupId, UnitId = UnitId,
        Name = name, IsPieceTracked = pieceTracked, IsSold = true, IsPurchased = true,
        StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate
    };

    private static Warehouse Warehouse(int id, int organizationId, string name) => new()
    {
        Id = id, OrganizationId = organizationId, Code = $"STOCK-WAREHOUSE-{id}", Name = name,
        StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate
    };

    private static WarehouseProduct WarehouseProduct(int warehouseId, int productId, decimal quantity, decimal reserved, decimal blocked) => new()
    {
        WarehouseId = warehouseId, ProductId = productId, UnitId = UnitId,
        Quantity = quantity, ReservedQuantity = reserved, BlockedQuantity = blocked,
        AvailableQuantity = quantity - reserved - blocked, CreatedAt = SeedDate
    };

    private static ProductTable ProductTable(int id, int productId, string serial, string marking) => new()
    {
        Id = id, ProductId = productId, SerialNumber = serial, MarkingNumber = marking, CreatedDate = SeedDate
    };

    private static WarehouseProductTable WarehouseProductTable(int warehouseId, int tableId, short statusId) => new()
    {
        WarehouseId = warehouseId, ProductTableId = tableId, StatusId = statusId, ReceivedDate = SeedDate, CreatedDate = SeedDate
    };

    private static WarehouseProductMovement Movement(long id, int organizationId, int warehouseId, int productId, long documentId, decimal quantity, short directionId, DateTime date) => new()
    {
        Id = id, OrganizationId = organizationId, WarehouseId = warehouseId, ProductId = productId,
        DocumentTypeId = DocumentTypeIdConst.PURCHASE, DocumentId = documentId,
        Quantity = quantity, DirectionId = directionId, MovementDate = date, CreatedDate = date
    };

    private static WarehouseProductBatch Batch(long id, int organizationId, int warehouseId, int productId, long movementId, decimal remaining, decimal unitCost, string number, DateTime date) => new()
    {
        Id = id, OrganizationId = organizationId, WarehouseId = warehouseId, ProductId = productId,
        ReceiptMovementId = movementId, BatchNumber = number, InitialQuantity = remaining,
        RemainingQuantity = remaining, UnitCost = unitCost, ReceivedDate = date, CreatedDate = date
    };

    private sealed class StubProductPriceCalculateService : IProductPriceCalculateService
    {
        public Task<Dictionary<int, ProductSalePriceDto>> GetSalePriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default) =>
            Task.FromResult(productIds.Distinct().ToDictionary(
                id => id,
                id => new ProductSalePriceDto { SalePrice = id == ProductId ? 10m : 20m }));

        public Task<Dictionary<int, ProductCostPriceDto>> GetCostPriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default) =>
            Task.FromResult(productIds.Distinct().ToDictionary(
                id => id,
                id => new ProductCostPriceDto { CostPrice = id == ProductId ? 4m : 5m }));

        public Task<Result<List<ProductTableSelectionDto>>> SelectInventoryAsync(
            int organizationId,
            int warehouseId,
            IReadOnlyCollection<ProductTableSelectionRequestDto> productLines,
            IReadOnlyCollection<int> selectedProductTableIds,
            CancellationToken ct = default) =>
            Task.FromResult(Result.Success(new List<ProductTableSelectionDto>()));
    }
}
