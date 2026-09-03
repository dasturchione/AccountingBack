using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.InventoryMovements;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using Infrastructure.Repositories;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;
using SharedKernel.Query;
using AppDbContext = global::Infrastructure.Persistence.AppDbContext;

namespace IntegrationTests.Features.Inv.InventoryMovements;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class InventoryMovementPersistenceTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 55001;
    private const int OrganizationId = 55001;
    private const int RegionId = 55001;
    private const int WarehouseId = 55001;
    private const int ProductId = 55001;
    private const int FirstProductTableId = 55001;
    private const int SecondProductTableId = 55002;
    private const int ThirdProductTableId = 55003;
    private const int FourthProductTableId = 55004;
    private const long DocumentId = 55001;
    private const long ReversibleDocumentId = 55002;
    private const long FirstSourceLineId = 55001;
    private const long SecondSourceLineId = 55002;
    private const long ThirdSourceLineId = 55003;
    private const long FourthSourceLineId = 55004;
    private const short UnitId = 30003;

    [Fact]
    public async Task PieceTrackedEntriesPreserveCurrentAggregateMovementAndBatchContract()
    {
        await SeedAsync();
        await using var provider = CreateProvider();
        await using var scope = provider.CreateAsyncScope();

        var result = await scope.ServiceProvider
            .GetRequiredService<IWarehouseProductBalanceService>()
            .ApplyInventoryEntriesAsync(
            [
                Entry(FirstProductTableId, FirstSourceLineId, 10m),
                Entry(SecondProductTableId, SecondSourceLineId, 20m)
            ]);

        Assert.True(result.IsSuccess);

        await using var context = fixture.CreateDbContext(User());
        var movements = await context.WarehouseProductMovements
            .Where(x => x.OrganizationId == OrganizationId &&
                        x.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT &&
                        x.DocumentId == DocumentId)
            .OrderBy(x => x.DocumentLineId)
            .ToListAsync();
        var batches = await context.WarehouseProductBatches
            .Where(x => x.OrganizationId == OrganizationId &&
                        x.ProductId == ProductId &&
                        x.ReceiptMovement.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT &&
                        x.ReceiptMovement.DocumentId == DocumentId)
            .OrderBy(x => x.UnitCost)
            .ToListAsync();

        var movement = Assert.Single(movements);
        Assert.Equal(FirstSourceLineId, movement.DocumentLineId);
        Assert.Equal(2m, movement.Quantity);
        var batch = Assert.Single(batches);
        Assert.Equal(15m, batch.UnitCost);

        var dtoQuery = scope.ServiceProvider.GetRequiredService<IQueryBuilder>()
            .For<WarehouseProductMovement>()
            .Where(x => x.OrganizationId == OrganizationId &&
                        x.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT &&
                        x.DocumentId == DocumentId)
            .As<InventoryMovementListDto>()
            .Build();
        var dto = Assert.Single(await scope.ServiceProvider
            .GetRequiredService<IQueryRepository<WarehouseProductMovement>>()
            .GetAllAsync(dtoQuery));
        Assert.Equal(30m, dto.Amount);
        Assert.Equal(SeedDate, dto.DocDate);
        Assert.Equal(FirstSourceLineId, dto.SourceLineId);
        Assert.Null(dto.ProductTableId);
        Assert.Null(dto.PostingBatchId);
        Assert.Null(dto.ReversalEntryId);
    }

    [Fact]
    public async Task DispatcherCanReverseAggregatedPieceTrackedMovement()
    {
        await SeedAsync();
        await using var provider = CreateProvider();
        await using var scope = provider.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IInventoryDispatcher>();
        var document = ReversibleDocument();

        var process = await dispatcher.ProcessAsync(document);
        Assert.True(process.IsSuccess);

        var reverse = await dispatcher.ReverseAsync(document);

        Assert.True(reverse.IsSuccess);
        await using var context = fixture.CreateDbContext(User());
        var movements = await context.WarehouseProductMovements
            .Where(x => x.OrganizationId == OrganizationId &&
                        x.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT &&
                        x.DocumentId == ReversibleDocumentId)
            .OrderBy(x => x.DirectionId)
            .ToListAsync();
        Assert.Equal(2, movements.Count);
        Assert.Equal(MovementDirectionIdConst.OUT, movements[0].DirectionId);
        Assert.Equal(MovementDirectionIdConst.IN, movements[1].DirectionId);
        Assert.All(movements, movement => Assert.Equal(2m, movement.Quantity));
        Assert.False(await context.WarehouseProductTables.AnyAsync(x =>
            x.ProductTableId == ThirdProductTableId || x.ProductTableId == FourthProductTableId));
    }

    private ServiceProvider CreateProvider() =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            User(),
            services =>
            {
                services.AddScoped<IWarehouseProductBalanceService, WarehouseProductBalanceService>();
                services.AddScoped<IInventoryDispatcher, InventoryDispatcher>();
                services.AddScoped<IInventoryDocumentHandler<PurchaseDoc>, PurchaseInventoryHandler>();
                services.AddScoped<IInventoryDocumentHandler<SaleDoc>, SaleInventoryHandler>();
                services.AddScoped<IInventoryDocumentHandler<RetailSaleDoc>, RetailSaleInventoryHandler>();
                services.AddScoped<IInventoryDocumentHandler<WarehouseTransferDoc>, WarehouseTransferInventoryHandler>();
                services.AddScoped<IInventoryDocumentHandler<InventoryAdjustmentDoc>, InventoryAdjustmentInventoryHandler>();
                services.AddScoped<IInventoryDocumentHandler<OpeningInventory>, OpeningInventoryHandler>();
            });

    private static IntegrationTestUserContext User() => new()
    {
        Id = 55001,
        UserKind = CurrentUserKind.SuperAdmin,
        LanguageId = LanguageIdConst.RU,
        TenantId = TenantId,
        OrganizationId = OrganizationId,
        AllowedOrganizationIds = [OrganizationId]
    };

    private static InventoryMovementEntry Entry(int productTableId, long sourceLineId, decimal amount) => new()
    {
        OrganizationId = OrganizationId,
        DocumentTypeId = DocumentTypeIdConst.INVENTORYADJUSTMENT,
        DocumentId = DocumentId,
        WarehouseId = WarehouseId,
        ProductId = ProductId,
        ProductTableId = productTableId,
        DirectionId = MovementDirectionIdConst.IN,
        Quantity = 1m,
        Amount = amount,
        DocDate = SeedDate,
        SourceLineId = sourceLineId
    };

    private static InventoryAdjustmentDoc ReversibleDocument()
    {
        var product = new Product
        {
            Id = ProductId,
            OrganizationId = OrganizationId,
            UnitId = UnitId,
            Name = "Tracked product",
            IsPieceTracked = true,
            IsPurchased = true,
            IsSold = true,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = SeedDate
        };
        var line = new InventoryAdjustmentLine
        {
            Id = ReversibleDocumentId,
            OwnerId = ReversibleDocumentId,
            ProductId = ProductId,
            Product = product,
            UnitId = UnitId,
            Quantity = 2m
        };
        line.InventoryAdjustmentDocTables.Add(new InventoryAdjustmentDocTable
        {
            Id = ThirdSourceLineId,
            OwnerId = line.Id,
            ProductTableId = ThirdProductTableId,
            CostPrice = 30m
        });
        line.InventoryAdjustmentDocTables.Add(new InventoryAdjustmentDocTable
        {
            Id = FourthSourceLineId,
            OwnerId = line.Id,
            ProductTableId = FourthProductTableId,
            CostPrice = 40m
        });

        var document = new InventoryAdjustmentDoc
        {
            Id = ReversibleDocumentId,
            OrganizationId = OrganizationId,
            DocNumber = "MOVEMENT-2",
            DocDate = SeedDate,
            WarehouseId = WarehouseId,
            AdjustmentType = "POSITIVE_ADJUSTMENT",
            DirectionId = MovementDirectionIdConst.IN,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = SeedDate
        };
        document.InventoryAdjustmentLines.Add(line);
        return document;
    }

    private async Task SeedAsync()
    {
        await using var context = fixture.CreateDbContext();

        if (!await context.States.AnyAsync(x => x.Id == StateIdConst.ACTIVE))
            context.States.Add(new State { Id = StateIdConst.ACTIVE, ShortName = "Active", FullName = "Active", CreatedDate = SeedDate });
        if (!await context.Regions.AnyAsync(x => x.Id == RegionId))
            context.Regions.Add(new Region { Id = RegionId, ShortName = "Movement region", FullName = "Movement region", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate });
        if (!await context.PlatformTenants.AnyAsync(x => x.Id == TenantId))
            context.PlatformTenants.Add(new PlatformTenant { Id = TenantId, Name = "Movement tenant", Slug = "inventory-movement-contract-tenant", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate });
        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(x => x.Id == OrganizationId))
            context.Organizations.Add(new Organization
            {
                Id = OrganizationId,
                ShortName = "Movement organization",
                FullName = "Movement organization",
                Inn = "550000001",
                RegionId = RegionId,
                IsParent = false,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate,
                TenantId = TenantId,
                SetupStatus = "completed"
            });
        if (!await context.Units.AnyAsync(x => x.Id == UnitId))
            context.Units.Add(new Unit { Id = UnitId, Code = "MOVEMENT-UNIT", Name = "Movement unit", StateId = StateIdConst.ACTIVE });
        if (!await context.Products.IgnoreQueryFilters().AnyAsync(x => x.Id == ProductId))
            context.Products.Add(new Product
            {
                Id = ProductId,
                OrganizationId = OrganizationId,
                UnitId = UnitId,
                Name = "Tracked product",
                IsPieceTracked = true,
                IsPurchased = true,
                IsSold = true,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        if (!await context.Warehouses.IgnoreQueryFilters().AnyAsync(x => x.Id == WarehouseId))
            context.Warehouses.Add(new Warehouse
            {
                Id = WarehouseId,
                OrganizationId = OrganizationId,
                Code = "MOVEMENT-WAREHOUSE",
                Name = "Movement warehouse",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        if (!await context.ProductTables.AnyAsync(x => x.Id == FirstProductTableId))
            context.ProductTables.AddRange(
                new ProductTable { Id = FirstProductTableId, ProductId = ProductId, MarkingNumber = "MOVEMENT-MARK-1", CreatedDate = SeedDate },
                new ProductTable { Id = SecondProductTableId, ProductId = ProductId, MarkingNumber = "MOVEMENT-MARK-2", CreatedDate = SeedDate },
                new ProductTable { Id = ThirdProductTableId, ProductId = ProductId, MarkingNumber = "MOVEMENT-MARK-3", CreatedDate = SeedDate },
                new ProductTable { Id = FourthProductTableId, ProductId = ProductId, MarkingNumber = "MOVEMENT-MARK-4", CreatedDate = SeedDate });
        if (!await context.DocumentStatuses.AnyAsync(x => x.Id == DocumentStatusIdConst.DRAFT))
            context.DocumentStatuses.Add(new DocumentStatus { Id = DocumentStatusIdConst.DRAFT, Code = "DRAFT", Name = "Draft", StateId = StateIdConst.ACTIVE });
        foreach (var directionId in new[] { MovementDirectionIdConst.OUT, MovementDirectionIdConst.IN })
            if (!await context.MovementDirections.AnyAsync(x => x.Id == directionId))
                context.MovementDirections.Add(new MovementDirection
                {
                    Id = directionId,
                    Code = directionId == MovementDirectionIdConst.IN ? "IN" : "OUT",
                    Name = directionId == MovementDirectionIdConst.IN ? "In" : "Out"
                });
        if (!await context.ProductTableStatuses.AnyAsync(x => x.Id == ProductTableStatusIdConst.IN_STOCK))
            context.ProductTableStatuses.Add(new ProductTableStatus
            {
                Id = ProductTableStatusIdConst.IN_STOCK,
                Code = "IN_STOCK",
                Name = "In stock",
                StateId = StateIdConst.ACTIVE
            });
        if (!await context.DocumentTypes.AnyAsync(x => x.Id == DocumentTypeIdConst.INVENTORYADJUSTMENT))
            context.DocumentTypes.Add(new DocumentType
            {
                Id = DocumentTypeIdConst.INVENTORYADJUSTMENT,
                Code = "INVENTORY_ADJUSTMENT",
                Name = "Inventory adjustment",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        if (!await context.InventoryAdjustmentDocs.IgnoreQueryFilters().AnyAsync(x => x.Id == DocumentId))
            context.InventoryAdjustmentDocs.AddRange(
                new InventoryAdjustmentDoc
                {
                    Id = DocumentId,
                    OrganizationId = OrganizationId,
                    DocNumber = "MOVEMENT-1",
                    DocDate = SeedDate,
                    WarehouseId = WarehouseId,
                    AdjustmentType = "POSITIVE_ADJUSTMENT",
                    DirectionId = MovementDirectionIdConst.IN,
                    StatusId = DocumentStatusIdConst.DRAFT,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = SeedDate
                },
                new InventoryAdjustmentDoc
                {
                    Id = ReversibleDocumentId,
                    OrganizationId = OrganizationId,
                    DocNumber = "MOVEMENT-2",
                    DocDate = SeedDate,
                    WarehouseId = WarehouseId,
                    AdjustmentType = "POSITIVE_ADJUSTMENT",
                    DirectionId = MovementDirectionIdConst.IN,
                    StatusId = DocumentStatusIdConst.DRAFT,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = SeedDate
                });

        await context.SaveChangesAsync();
    }
}
