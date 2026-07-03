using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.InventoryAdjustments;
using Application.Features.InventoryCounts;
using Application.Features.InventoryRegisterBalances;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public class InventoryCountCrudTests
{
    [Fact]
    public async Task Create_ShouldPersistDraftDocument()
    {
        var fixture = InventoryCountCrudFixture.Create();

        var result = await fixture.Service.CreateAsync(fixture.CreateDto);

        Assert.True(result.IsSuccess);
        var created = Assert.Single(fixture.Docs, x => x.Id == result.Value);
        Assert.Equal(DocumentStatusIdConst.DRAFT, created.StatusId);
        Assert.Equal(StateIdConst.ACTIVE, created.StateId);
    }

    [Fact]
    public async Task Update_WithCountCompleted_ShouldMoveToPending()
    {
        var fixture = InventoryCountCrudFixture.Create();
        var doc = InventoryCountTestData.BuildDoc();
        fixture.Docs.Add(doc);
        fixture.UpdateDto.IsCountCompleted = true;

        var result = await fixture.Service.UpdateAsync(doc.Id, fixture.UpdateDto);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.PENDING, doc.StatusId);
        Assert.NotNull(doc.CountCompletedAt);
    }

    [Fact]
    public async Task Delete_ShouldRejectNonDraft()
    {
        var fixture = InventoryCountCrudFixture.Create();
        fixture.Docs.Add(InventoryCountTestData.BuildDoc(id: 12, statusId: DocumentStatusIdConst.PENDING));

        var result = await fixture.Service.DeleteAsync(12);

        Assert.False(result.IsSuccess);
        Assert.Equal("InventoryCount.CannotDeleteInCurrentStatus", result.Error.Code);
    }

    [Fact]
    public async Task Create_ShouldRejectSimultaneousCountForWarehouse()
    {
        var fixture = InventoryCountCrudFixture.Create();
        fixture.Docs.Add(InventoryCountTestData.BuildDoc(id: 15, statusId: DocumentStatusIdConst.DRAFT));

        var result = await fixture.Service.CreateAsync(fixture.CreateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("InventoryCount.SimultaneousCountExists", result.Error.Code);
    }

    [Fact]
    public async Task GetById_ShouldRespectOrganizationBoundary()
    {
        var fixture = InventoryCountCrudFixture.Create();
        fixture.Docs.Add(InventoryCountTestData.BuildDoc(id: 20));
        fixture.Docs[0].OrganizationId = 99;

        var result = await fixture.Service.GetByIdAsync(20);

        Assert.False(result.IsSuccess);
        Assert.Equal("InventoryCount.NotFound", result.Error.Code);
    }
}

public class InventoryCountLifecycleTests
{
    [Fact]
    public async Task GetDifferences_ShouldReturnCorrectMissingAndFound()
    {
        var fixture = InventoryCountLifecycleFixture.Create();

        var result = await fixture.Service.GetDifferencesAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        var difference = Assert.Single(result.Value);
        Assert.Equal(2, difference.ExpectedQuantity);
        Assert.Equal(2, difference.CountedQuantity);
        Assert.Equal(1, difference.CorrectQuantity);
        Assert.Equal(1, difference.MissingQuantity);
        Assert.Equal(1, difference.FoundQuantity);
        Assert.Single(difference.MissingProductTableIds);
        Assert.Single(difference.FoundItems);
    }

    [Fact]
    public async Task Confirm_ShouldCreatePostingBatchAndLinkedAdjustments()
    {
        var fixture = InventoryCountLifecycleFixture.Create();

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.Doc.StatusId);
        Assert.Single(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.POSTED);
        Assert.NotNull(fixture.Doc.PositiveAdjustmentDocId);
        Assert.NotNull(fixture.Doc.NegativeAdjustmentDocId);
        Assert.Equal(2, fixture.AdjustmentLifecycle.ConfirmedIds.Count);
        Assert.Equal(2, fixture.RegisterBalances.Count(x => x.ReversalEntryId == null));
    }

    [Fact]
    public async Task Confirm_Twice_ShouldBeIdempotent()
    {
        var fixture = InventoryCountLifecycleFixture.Create();

        var first = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);
        var second = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Single(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.POSTED);
        Assert.Equal(2, fixture.AdjustmentLifecycle.ConfirmedIds.Count);
    }

    [Fact]
    public async Task Cancel_ShouldReverseOwnBatchAndLinkedAdjustments()
    {
        var fixture = InventoryCountLifecycleFixture.Create();
        await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        var result = await fixture.LifecycleService.CancelAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.CANCELLED, fixture.Doc.StatusId);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSED);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSAL);
        Assert.Equal(2, fixture.AdjustmentLifecycle.CancelledIds.Count);
        Assert.Equal(4, fixture.RegisterBalances.Count);
    }

    [Fact]
    public async Task Cancel_Twice_ShouldBeIdempotent()
    {
        var fixture = InventoryCountLifecycleFixture.Create();
        await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);
        await fixture.LifecycleService.CancelAsync(fixture.Doc.Id);

        var result = await fixture.LifecycleService.CancelAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, fixture.PostingBatches.Count);
    }

    [Fact]
    public async Task Confirm_ShouldRejectClosedPeriod()
    {
        var fixture = InventoryCountLifecycleFixture.Create();
        fixture.PeriodValidator.AlwaysClosed = true;

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPeriod.Closed", result.Error.Code);
    }

    [Fact]
    public async Task ActiveInventoryCountGuard_ShouldBlockWarehouseOperation()
    {
        var data = InventoryCountTestData.CreateBaseData();
        data.Docs.Add(InventoryCountTestData.BuildDoc(id: 50, statusId: DocumentStatusIdConst.PENDING));

        var guard = new ActiveInventoryCountGuardService(
            new FakeCountQueryRepository<InventoryCountDoc>(data.Docs),
            new FakeCountUserContext());

        var result = await guard.EnsureWarehouseIsNotBlockedAsync(8, 1, "WarehouseTransferConfirm");

        Assert.False(result.IsSuccess);
        Assert.Equal("Common.WarehouseBlockedByInventoryCount", result.Error.Code);
    }
}

file sealed class InventoryCountCrudFixture
{
    public required InventoryCountService Service { get; init; }
    public required List<InventoryCountDoc> Docs { get; init; }
    public required InventoryCountCreateDto CreateDto { get; init; }
    public required InventoryCountUpdateDto UpdateDto { get; init; }

    public static InventoryCountCrudFixture Create()
    {
        var data = InventoryCountTestData.CreateBaseData();
        var service = new InventoryCountService(
            new FakeCountUserContext(),
            new FakeCountInventoryReadDbContext(data.ProductTables, data.Products),
            new FakeCountQueryBuilder(),
            new FakeCountAuditLogService(),
            new FakeCountLifecycleService(),
            new FakeCountDocNumberGenerator(),
            new FakeCountQueryRepository<InventoryCountDoc>(data.Docs),
            new FakeCountCommandRepository<InventoryCountDoc>(data.Docs, x => x.Id = x.Id == 0 ? data.NextCountDocId++ : x.Id),
            new FakeCountCommandRepository<InventoryCountLine>(),
            new FakeCountCommandRepository<InventoryCountDocTable>(),
            new FakeCountQueryRepository<PostingBatch>(data.PostingBatches),
            new FakeCountQueryRepository<RegisterBalance>(data.RegisterBalances),
            new FakeCountQueryRepository<Organization>(data.Organizations),
            new FakeCountQueryRepository<Warehouse>(data.Warehouses),
            new FakeCountQueryRepository<Product>(data.Products),
            new FakeCountQueryRepository<Unit>(data.Units),
            new FakeCountQueryRepository<ProductTable>(data.ProductTables),
            new FakeCountQueryRepository<InventoryAdjustmentDoc>(data.InventoryAdjustments),
            NullLogger<InventoryCountService>.Instance,
            new FakeCountUnitOfWork());

        return new InventoryCountCrudFixture
        {
            Service = service,
            Docs = data.Docs,
            CreateDto = InventoryCountTestData.BuildCreateDto(),
            UpdateDto = InventoryCountTestData.BuildUpdateDto()
        };
    }
}

file sealed class InventoryCountLifecycleFixture
{
    public required InventoryCountService Service { get; init; }
    public required InventoryCountLifecycleService LifecycleService { get; init; }
    public required InventoryCountDoc Doc { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }
    public required List<RegisterBalance> RegisterBalances { get; init; }
    public required List<InventoryAdjustmentDoc> InventoryAdjustments { get; init; }
    public required FakeCountAdjustmentLifecycleService AdjustmentLifecycle { get; init; }
    public required FakeCountPeriodValidator PeriodValidator { get; init; }

    public static InventoryCountLifecycleFixture Create()
    {
        var data = InventoryCountTestData.CreateBaseData();
        var doc = InventoryCountTestData.BuildLifecycleDoc(data.ProductTables[0], data.ProductTables[1], data.Products[0], data.Units[0], data.Warehouses[0]);
        data.Docs.Add(doc);

        var periodValidator = new FakeCountPeriodValidator();
        var adjustmentLifecycle = new FakeCountAdjustmentLifecycleService(data.InventoryAdjustments, data.ProductTables, data.RegisterBalances);

        var service = new InventoryCountService(
            new FakeCountUserContext(),
            new FakeCountInventoryReadDbContext(data.ProductTables, data.Products),
            new FakeCountQueryBuilder(),
            new FakeCountAuditLogService(),
            new FakeCountLifecycleService(),
            new FakeCountDocNumberGenerator(),
            new FakeCountQueryRepository<InventoryCountDoc>(data.Docs),
            new FakeCountCommandRepository<InventoryCountDoc>(data.Docs),
            new FakeCountCommandRepository<InventoryCountLine>(),
            new FakeCountCommandRepository<InventoryCountDocTable>(),
            new FakeCountQueryRepository<PostingBatch>(data.PostingBatches),
            new FakeCountQueryRepository<RegisterBalance>(data.RegisterBalances),
            new FakeCountQueryRepository<Organization>(data.Organizations),
            new FakeCountQueryRepository<Warehouse>(data.Warehouses),
            new FakeCountQueryRepository<Product>(data.Products),
            new FakeCountQueryRepository<Unit>(data.Units),
            new FakeCountQueryRepository<ProductTable>(data.ProductTables),
            new FakeCountQueryRepository<InventoryAdjustmentDoc>(data.InventoryAdjustments),
            NullLogger<InventoryCountService>.Instance,
            new FakeCountUnitOfWork());

        var lifecycle = new InventoryCountLifecycleService(
            new FakeCountUserContext(),
            new FakeCountInventoryReadDbContext(data.ProductTables, data.Products),
            new FakeCountQueryBuilder(),
            new FakeCountPostingLock(),
            periodValidator,
            new FakeCountAuditLogService(),
            new FakeCountDocNumberGenerator(),
            adjustmentLifecycle,
            new FakeCountQueryRepository<InventoryCountDoc>(data.Docs),
            new FakeCountCommandRepository<InventoryCountDoc>(data.Docs),
            new FakeCountCommandRepository<PostingBatch>(data.PostingBatches, x => x.Id = x.Id == 0 ? data.NextPostingBatchId++ : x.Id),
            new FakeCountQueryRepository<PostingBatch>(data.PostingBatches),
            new FakeCountCommandRepository<InventoryAdjustmentDoc>(data.InventoryAdjustments, x => x.Id = x.Id == 0 ? data.NextAdjustmentDocId++ : x.Id),
            new FakeCountQueryRepository<InventoryAdjustmentDoc>(data.InventoryAdjustments),
            new FakeCountCommandRepository<ProductTable>(data.ProductTables, x => x.Id = x.Id == 0 ? data.NextProductTableId++ : x.Id),
            new FakeCountQueryRepository<ProductTable>(data.ProductTables),
            new FakeCountQueryRepository<RegisterBalance>(data.RegisterBalances),
            NullLogger<InventoryCountLifecycleService>.Instance,
            new FakeCountUnitOfWork());

        return new InventoryCountLifecycleFixture
        {
            Service = service,
            LifecycleService = lifecycle,
            Doc = doc,
            PostingBatches = data.PostingBatches,
            RegisterBalances = data.RegisterBalances,
            InventoryAdjustments = data.InventoryAdjustments,
            AdjustmentLifecycle = adjustmentLifecycle,
            PeriodValidator = periodValidator
        };
    }
}

file static class InventoryCountTestData
{
    public static InventoryCountCreateDto BuildCreateDto() => new()
    {
        DocDate = DateTime.Today,
        WarehouseId = 1,
        Comment = "draft",
        Lines =
        [
            new InventoryCountLineRequestDto
            {
                ProductId = 100,
                UnitId = 1,
                CountedQuantity = 2,
                DefaultCostPrice = 20m,
                Items =
                [
                    new InventoryCountTableRequestDto { ProductTableId = 500, Barcode = "B-100", CostPrice = 20m },
                    new InventoryCountTableRequestDto { Barcode = "B-100-F", SerialNumber = "S-F", MarkingNumber = "M-F", CostPrice = 22m }
                ]
            }
        ]
    };

    public static InventoryCountUpdateDto BuildUpdateDto() => new()
    {
        DocDate = DateTime.Today,
        WarehouseId = 1,
        Comment = "updated",
        StateId = StateIdConst.ACTIVE,
        Lines = BuildCreateDto().Lines
    };

    public static FakeCountData CreateBaseData()
    {
        var organizations = new List<Organization>
        {
            new() { Id = 8, ShortName = "Org", FullName = "Org", Inn = "123", RegionId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today, SetupStatus = "DONE" }
        };
        var warehouses = new List<Warehouse>
        {
            new() { Id = 1, OrganizationId = 8, Name = "Main", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today, IsMain = true }
        };
        var units = new List<Unit> { new() { Id = 1, Code = "pcs", Name = "pcs", StateId = StateIdConst.ACTIVE } };
        var products = new List<Product>
        {
            new() { Id = 100, OrganizationId = 8, Name = "Product", UnitId = 1, Unit = units[0], IsService = false, IsPieceTracked = true, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today, Barcode = "B-100" }
        };
        var productTables = new List<ProductTable>
        {
            new() { Id = 500, ProductId = 100, Product = products[0], OrganizationId = 8, CurrentWarehouseId = 1, StateId = StateIdConst.ACTIVE, StatusId = ProductTableStatusIdConst.IN_STOCK, CreatedDate = DateTime.Today, SerialNumber = "S-1", MarkingNumber = "M-1" },
            new() { Id = 501, ProductId = 100, Product = products[0], OrganizationId = 8, CurrentWarehouseId = 1, StateId = StateIdConst.ACTIVE, StatusId = ProductTableStatusIdConst.IN_STOCK, CreatedDate = DateTime.Today, SerialNumber = "S-2", MarkingNumber = "M-2" }
        };

        return new FakeCountData
        {
            Docs = [],
            Organizations = organizations,
            Warehouses = warehouses,
            Products = products,
            Units = units,
            ProductTables = productTables,
            PostingBatches = [],
            RegisterBalances = [],
            InventoryAdjustments = [],
            NextCountDocId = 100,
            NextPostingBatchId = 1,
            NextAdjustmentDocId = 1000,
            NextProductTableId = 600
        };
    }

    public static InventoryCountDoc BuildDoc(long id = 10, short statusId = DocumentStatusIdConst.DRAFT) =>
        new()
        {
            Id = id,
            OrganizationId = 8,
            DocNumber = $"ICT-{id}",
            DocDate = DateTime.Today,
            WarehouseId = 1,
            Warehouse = new Warehouse { Id = 1, OrganizationId = 8, Name = "Main", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            StatusId = statusId,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        };

    public static InventoryCountDoc BuildLifecycleDoc(ProductTable countedTable, ProductTable missingTable, Product product, Unit unit, Warehouse warehouse)
    {
        var foundItem = new InventoryCountDocTable
        {
            Id = 901,
            Barcode = "B-100-F",
            SerialNumber = "S-F",
            MarkingNumber = "M-F",
            CostPrice = 22m
        };

        var countedExisting = new InventoryCountDocTable
        {
            Id = 900,
            ProductTableId = countedTable.Id,
            ProductTable = countedTable,
            Barcode = product.Barcode,
            SerialNumber = countedTable.SerialNumber,
            MarkingNumber = countedTable.MarkingNumber,
            CostPrice = 20m
        };

        var line = new InventoryCountLine
        {
            Id = 800,
            ProductId = product.Id,
            Product = product,
            UnitId = unit.Id,
            Unit = unit,
            CountedQuantity = 2,
            DefaultCostPrice = 22m,
            InventoryCountDocTables = [countedExisting, foundItem]
        };

        var doc = new InventoryCountDoc
        {
            Id = 700,
            OrganizationId = 8,
            DocNumber = "ICT-700",
            DocDate = DateTime.Today,
            WarehouseId = warehouse.Id,
            Warehouse = warehouse,
            StatusId = DocumentStatusIdConst.PENDING,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today,
            CountCompletedAt = DateTime.Today,
            CountCompletedByUserId = 10,
            InventoryCountLines = [line]
        };

        line.OwnerId = doc.Id;
        line.Owner = doc;
        countedExisting.OwnerId = line.Id;
        countedExisting.Owner = line;
        foundItem.OwnerId = line.Id;
        foundItem.Owner = line;

        return doc;
    }
}

file sealed class FakeCountData
{
    public required List<InventoryCountDoc> Docs { get; init; }
    public required List<Organization> Organizations { get; init; }
    public required List<Warehouse> Warehouses { get; init; }
    public required List<Product> Products { get; init; }
    public required List<Unit> Units { get; init; }
    public required List<ProductTable> ProductTables { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }
    public required List<RegisterBalance> RegisterBalances { get; init; }
    public required List<InventoryAdjustmentDoc> InventoryAdjustments { get; init; }
    public required long NextCountDocId { get; set; }
    public required long NextPostingBatchId { get; set; }
    public required long NextAdjustmentDocId { get; set; }
    public required int NextProductTableId { get; set; }
}

file sealed class FakeCountUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class FakeCountUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeCountDocNumberGenerator : IDocNumberGenerator
{
    public Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-{Guid.NewGuid():N}".Substring(0, 12));
}

file sealed class FakeCountInventoryReadDbContext : IInventoryReadDbContext
{
    public FakeCountInventoryReadDbContext(List<ProductTable> productTables, List<Product> products)
    {
        ProductTables = productTables.AsQueryable();
        Products = products.AsQueryable();
    }

    public IQueryable<ProductTable> ProductTables { get; }
    public IQueryable<Product> Products { get; }
}

file sealed class FakeCountAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class FakeCountLifecycleService : IInventoryCountLifecycleService
{
    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class FakeCountPeriodValidator : IAccountingPeriodValidator
{
    public bool AlwaysClosed { get; set; }
    public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) =>
        Task.FromResult(AlwaysClosed
            ? Result.Failure(Error.Business("AccountingPeriod.Closed", $"Accounting period {date:yyyy-MM} is closed."))
            : Result.Success());
}

file sealed class FakeCountPostingLock : IDocumentPostingLock
{
    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeCountAdjustmentLifecycleService : IInventoryAdjustmentLifecycleService
{
    private readonly List<InventoryAdjustmentDoc> _adjustments;
    private readonly List<ProductTable> _productTables;
    private readonly List<RegisterBalance> _registerBalances;
    private long _nextRegisterId = 1;
    private int _nextProductTableId = 800;

    public FakeCountAdjustmentLifecycleService(List<InventoryAdjustmentDoc> adjustments, List<ProductTable> productTables, List<RegisterBalance> registerBalances)
    {
        _adjustments = adjustments;
        _productTables = productTables;
        _registerBalances = registerBalances;
    }

    public List<long> ConfirmedIds { get; } = new();
    public List<long> CancelledIds { get; } = new();

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default)
    {
        var doc = _adjustments.Single(x => x.Id == id);
        if (doc.StatusId == DocumentStatusIdConst.POSTED)
            return Task.FromResult(Result.Success());

        foreach (var line in doc.InventoryAdjustmentLines)
        {
            foreach (var table in line.InventoryAdjustmentDocTables)
            {
                if (doc.AdjustmentType == "POSITIVE_ADJUSTMENT" && !table.ProductTableId.HasValue)
                {
                    var created = new ProductTable
                    {
                        Id = _nextProductTableId++,
                        ProductId = line.ProductId,
                        Product = line.Product,
                        OrganizationId = doc.OrganizationId,
                        CurrentWarehouseId = doc.WarehouseId,
                        StatusId = ProductTableStatusIdConst.IN_STOCK,
                        StateId = StateIdConst.ACTIVE,
                        CreatedDate = DateTime.Now
                    };
                    _productTables.Add(created);
                    table.ProductTableId = created.Id;
                    table.ProductTable = created;
                    table.WasCreated = true;
                }
                else if (table.ProductTableId.HasValue)
                {
                    var existing = _productTables.Single(x => x.Id == table.ProductTableId.Value);
                    if (doc.AdjustmentType == "NEGATIVE_ADJUSTMENT")
                    {
                        existing.StatusId = ProductTableStatusIdConst.BLOCKED;
                    }
                }

                _registerBalances.Add(new RegisterBalance
                {
                    Id = _nextRegisterId++,
                    OrganizationId = doc.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.INVENTORYADJUSTMENT,
                    DocumentId = doc.Id,
                    WarehouseId = doc.WarehouseId,
                    ProductId = line.ProductId,
                    ProductTableId = table.ProductTableId,
                    OperationTypeId = doc.AdjustmentType == "NEGATIVE_ADJUSTMENT" ? OperationTypeIdConst.OUT : OperationTypeIdConst.IN,
                    Quantity = 1,
                    Amount = table.CostPrice,
                    DocDate = doc.DocDate,
                    CreatedDate = DateTime.Now
                });
            }
        }

        doc.StatusId = DocumentStatusIdConst.POSTED;
        ConfirmedIds.Add(id);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> CancelAsync(long id, CancellationToken ct = default)
    {
        var doc = _adjustments.Single(x => x.Id == id);
        if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
            return Task.FromResult(Result.Success());

        var originalEntries = _registerBalances.Where(x => x.DocumentId == id && x.ReversalEntryId == null).ToList();
        foreach (var entry in originalEntries)
        {
            _registerBalances.Add(new RegisterBalance
            {
                Id = _nextRegisterId++,
                OrganizationId = entry.OrganizationId,
                DocumentTypeId = entry.DocumentTypeId,
                DocumentId = entry.DocumentId,
                WarehouseId = entry.WarehouseId,
                ProductId = entry.ProductId,
                ProductTableId = entry.ProductTableId,
                OperationTypeId = entry.OperationTypeId == OperationTypeIdConst.IN ? OperationTypeIdConst.OUT : OperationTypeIdConst.IN,
                Quantity = entry.Quantity,
                Amount = entry.Amount,
                DocDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                ReversalEntryId = entry.Id
            });
        }

        foreach (var table in doc.InventoryAdjustmentLines.SelectMany(x => x.InventoryAdjustmentDocTables))
        {
            if (!table.ProductTableId.HasValue)
                continue;

            var productTable = _productTables.Single(x => x.Id == table.ProductTableId.Value);
            if (table.WasCreated)
            {
                productTable.CurrentWarehouseId = null;
                productTable.StateId = StateIdConst.PASSIVE;
                productTable.StatusId = ProductTableStatusIdConst.BLOCKED;
            }
            else
            {
                productTable.CurrentWarehouseId = doc.WarehouseId;
                productTable.StateId = StateIdConst.ACTIVE;
                productTable.StatusId = ProductTableStatusIdConst.IN_STOCK;
            }
        }

        doc.StatusId = DocumentStatusIdConst.CANCELLED;
        CancelledIds.Add(id);
        return Task.FromResult(Result.Success());
    }
}

file sealed class FakeCountCommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity>? _store;
    private readonly Action<TEntity>? _onCreate;

    public FakeCountCommandRepository(List<TEntity>? store = null, Action<TEntity>? onCreate = null)
    {
        _store = store;
        _onCreate = onCreate;
    }

    public Task CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        _onCreate?.Invoke(entity);
        _store?.Add(entity);
        return Task.CompletedTask;
    }

    public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            _onCreate?.Invoke(entity);
            _store?.Add(entity);
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        if (_store == null)
            return Task.CompletedTask;
        _store.RemoveAll(new Predicate<TEntity>(predicate.Compile()));
        return Task.CompletedTask;
    }
    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeCountQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public FakeCountQueryRepository(List<TEntity> data)
    {
        _data = data;
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Any(predicate));

    public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).FirstOrDefault());

    public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(default(TResult));

    public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).ToList());

    public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(new List<TResult>());

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(new PagedList<TEntity>(
            _data.AsQueryable().Where(specification.Criteria).Skip(specification.Skip).Take(specification.Take ?? 50).ToList(),
            _data.AsQueryable().Count(specification.Criteria)));

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(new PagedList<TResult>([], 0));
}

file sealed class FakeCountQueryBuilder : IQueryBuilder
{
    private static readonly FakeCountQueryBuilderResolver Resolver = new();

    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = Resolver });

    public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true };

    public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default! };

    public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options) where TEntity : class where TOptions : SharedKernel.Filters.IPaginationFilter =>
        new() { Criteria = _ => true, Skip = 0, Take = 50 };

    public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options) where TEntity : class where TOptions : SharedKernel.Filters.IPaginationFilter =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default!, Skip = 0, Take = 50 };
}

file sealed class FakeCountQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new FakeCountProjectionBuilder<TEntity, TResult>();
}

file sealed class FakeCountProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
