using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.InventoryCounts;
using Application.Features.InventoryRegisterBalances;
using Application.Features.WarehouseTransfers;
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

public class WarehouseTransferPhase1Tests
{
    [Fact]
    public async Task Create_ShouldPersistDraftDocument()
    {
        var fixture = WarehouseTransferCrudFixture.Create();

        var result = await fixture.Service.CreateAsync(fixture.CreateDto);

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.DocCommand.CreatedEntities);
        var created = fixture.DocCommand.CreatedEntities.Single();
        Assert.Equal(DocumentStatusIdConst.DRAFT, created.StatusId);
        Assert.Equal(StateIdConst.ACTIVE, created.StateId);
        Assert.Single(created.WarehouseTransferLines);
        Assert.Single(created.WarehouseTransferLines.Single().WarehouseTransferDocTables);
        Assert.Equal(1, fixture.UnitOfWork.BeginCount);
        Assert.Equal(1, fixture.UnitOfWork.CommitCount);
    }

    [Fact]
    public async Task Update_ShouldRejectNonDraftDocument()
    {
        var fixture = WarehouseTransferCrudFixture.Create();
        fixture.Docs.Add(WarehouseTransferTestData.BuildDoc(statusId: DocumentStatusIdConst.POSTED));

        var result = await fixture.Service.UpdateAsync(10, fixture.UpdateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("WarehouseTransfer.CannotUpdateInCurrentStatus", result.Error.Code);
    }

    [Fact]
    public async Task Delete_ShouldRejectNonDraftDocument()
    {
        var fixture = WarehouseTransferCrudFixture.Create();
        fixture.Docs.Add(WarehouseTransferTestData.BuildDoc(id: 11, statusId: DocumentStatusIdConst.CANCELLED));

        var result = await fixture.Service.DeleteAsync(11);

        Assert.False(result.IsSuccess);
        Assert.Equal("WarehouseTransfer.CannotDeleteInCurrentStatus", result.Error.Code);
    }

    [Fact]
    public async Task Create_ShouldRejectDuplicateProductTableRows()
    {
        var fixture = WarehouseTransferCrudFixture.Create();
        fixture.CreateDto.Lines[0].Items.Add(new WarehouseTransferTableRequestDto
        {
            ProductTableId = fixture.CreateDto.Lines[0].Items[0].ProductTableId,
            CostPrice = 25m
        });
        fixture.CreateDto.Lines[0].Quantity = 2;

        var result = await fixture.Service.CreateAsync(fixture.CreateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("WarehouseTransfer.DuplicateProductTable", result.Error.Code);
        Assert.Equal(1, fixture.UnitOfWork.RollbackCount);
    }

    [Fact]
    public async Task Delete_ShouldSoftDeleteDraftDocument()
    {
        var fixture = WarehouseTransferCrudFixture.Create();
        fixture.Docs.Add(WarehouseTransferTestData.BuildDoc(id: 12));

        var result = await fixture.Service.DeleteAsync(12);

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.DocCommand.UpdatedEntities);
        Assert.Equal(StateIdConst.PASSIVE, fixture.DocCommand.UpdatedEntities.Single().StateId);
        Assert.Equal(1, fixture.LineCommand.DeletePredicateCount);
        Assert.Equal(1, fixture.TableCommand.DeletePredicateCount);
    }

    [Fact]
    public void Validator_ShouldRejectSameWarehouse()
    {
        var validator = new WarehouseTransferCreateDtoValidator();
        var dto = WarehouseTransferTestData.BuildCreateDto();
        dto.DestinationWarehouseId = dto.SourceWarehouseId;

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}

public class WarehouseTransferLifecycleTests
{
    [Fact]
    public async Task Confirm_ShouldCreatePostingBatch_MoveWarehouse_AndCreateInventoryEntries()
    {
        var fixture = WarehouseTransferLifecycleFixture.Create();

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, fixture.PostingLock.AcquireCount);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.Doc.StatusId);
        Assert.Equal(2, fixture.ProductTable.CurrentWarehouseId);
        Assert.Single(fixture.PostingBatches);
        Assert.Equal(PostingBatchStatusConst.POSTED, fixture.PostingBatches.Single().Status);
        Assert.Equal(2, fixture.InventoryEntries.Count);
        Assert.Contains(fixture.InventoryEntries, x => x.OperationTypeId == OperationTypeIdConst.OUT && x.WarehouseId == 1);
        Assert.Contains(fixture.InventoryEntries, x => x.OperationTypeId == OperationTypeIdConst.IN && x.WarehouseId == 2);
        Assert.All(fixture.InventoryEntries, x => Assert.Equal(fixture.PostingBatches.Single().Id, x.PostingBatchId));
    }

    [Fact]
    public async Task Confirm_ShouldBeIdempotent_WhenAlreadyPosted()
    {
        var fixture = WarehouseTransferLifecycleFixture.Create();
        fixture.Doc.StatusId = DocumentStatusIdConst.POSTED;
        fixture.PostingBatches.Add(new PostingBatch
        {
            Id = 1,
            OrganizationId = fixture.Doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.WAREHOUSETRANSFER,
            DocumentId = fixture.Doc.Id,
            Status = PostingBatchStatusConst.POSTED,
            PostedAt = DateTime.Now
        });

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.PostingBatches);
        Assert.Empty(fixture.InventoryEntries);
    }

    [Fact]
    public async Task Confirm_ShouldRejectClosedAccountingPeriod()
    {
        var fixture = WarehouseTransferLifecycleFixture.Create();
        fixture.PeriodValidator.IsOpen = false;

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPeriod.Closed", result.Error.Code);
    }

    [Fact]
    public async Task Confirm_ShouldRejectInactiveWarehouse()
    {
        var fixture = WarehouseTransferLifecycleFixture.Create();
        fixture.SourceWarehouse.StateId = StateIdConst.PASSIVE;

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("WarehouseTransfer.WarehouseInactive", result.Error.Code);
    }

    [Fact]
    public async Task Confirm_ShouldRejectReservedProductTable()
    {
        var fixture = WarehouseTransferLifecycleFixture.Create();
        fixture.ProductTable.StatusId = ProductTableStatusIdConst.RESERVED;

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("WarehouseTransfer.ProductTableUnavailable", result.Error.Code);
    }

    [Fact]
    public async Task Cancel_ShouldCreateReversalBatch_RestoreWarehouse_AndReverseInventory()
    {
        var fixture = WarehouseTransferLifecycleFixture.Create();
        var confirm = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);
        Assert.True(confirm.IsSuccess);

        var result = await fixture.LifecycleService.CancelAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, fixture.PostingLock.AcquireCount);
        Assert.Equal(DocumentStatusIdConst.CANCELLED, fixture.Doc.StatusId);
        Assert.Equal(1, fixture.ProductTable.CurrentWarehouseId);
        Assert.Equal(2, fixture.PostingBatches.Count);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSAL);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSED);
        Assert.Equal(4, fixture.InventoryEntries.Count);
        Assert.Equal(2, fixture.InventoryEntries.Count(x => x.ReversalEntryId.HasValue));
    }

    [Fact]
    public async Task Cancel_ShouldBeIdempotent_WhenAlreadyCancelled()
    {
        var fixture = WarehouseTransferLifecycleFixture.Create();
        fixture.Doc.StatusId = DocumentStatusIdConst.CANCELLED;

        var result = await fixture.LifecycleService.CancelAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(fixture.PostingBatches);
        Assert.Empty(fixture.InventoryEntries);
    }
}

file sealed class WarehouseTransferCrudFixture
{
    public required WarehouseTransferService Service { get; init; }
    public required FakeUserContext UserContext { get; init; }
    public required FakeUnitOfWork UnitOfWork { get; init; }
    public required FakeCommandRepository<WarehouseTransferDoc> DocCommand { get; init; }
    public required FakeCommandRepository<WarehouseTransferLine> LineCommand { get; init; }
    public required FakeCommandRepository<WarehouseTransferDocTable> TableCommand { get; init; }
    public required List<WarehouseTransferDoc> Docs { get; init; }
    public required WarehouseTransferCreateDto CreateDto { get; init; }
    public required WarehouseTransferUpdateDto UpdateDto { get; init; }

    public static WarehouseTransferCrudFixture Create()
    {
        var data = WarehouseTransferTestData.CreateBaseData();
        var userContext = new FakeUserContext();
        var unitOfWork = new FakeUnitOfWork();
        var docCommand = new FakeCommandRepository<WarehouseTransferDoc>(data.Docs, entity => entity.Id = entity.Id == 0 ? 999 : entity.Id);
        var lineCommand = new FakeCommandRepository<WarehouseTransferLine>();
        var tableCommand = new FakeCommandRepository<WarehouseTransferDocTable>();

        var service = new WarehouseTransferService(
            userContext,
            new FakeQueryBuilder(),
            new FakeAuditLogService(),
            new FakeWarehouseTransferLifecycleService(),
            new FakeDocNumberGenerator(),
            new FakeQueryRepository<WarehouseTransferDoc>(data.Docs),
            docCommand,
            lineCommand,
            tableCommand,
            new FakeQueryRepository<PostingBatch>(new List<PostingBatch>()),
            new FakeQueryRepository<RegisterBalance>(new List<RegisterBalance>()),
            new FakeQueryRepository<Organization>(data.Organizations),
            new FakeQueryRepository<Warehouse>(data.Warehouses),
            new FakeQueryRepository<Product>(data.Products),
            new FakeQueryRepository<Unit>(data.Units),
            new FakeQueryRepository<ProductTable>(data.ProductTables),
            NullLogger<WarehouseTransferService>.Instance,
            unitOfWork);

        return new WarehouseTransferCrudFixture
        {
            Service = service,
            UserContext = userContext,
            UnitOfWork = unitOfWork,
            DocCommand = docCommand,
            LineCommand = lineCommand,
            TableCommand = tableCommand,
            Docs = data.Docs,
            CreateDto = WarehouseTransferTestData.BuildCreateDto(),
            UpdateDto = WarehouseTransferTestData.BuildUpdateDto()
        };
    }
}

file sealed class WarehouseTransferLifecycleFixture
{
    public required WarehouseTransferLifecycleService LifecycleService { get; init; }
    public required FakePostingLock PostingLock { get; init; }
    public required FakeAccountingPeriodValidator PeriodValidator { get; init; }
    public required WarehouseTransferDoc Doc { get; init; }
    public required Warehouse SourceWarehouse { get; init; }
    public required Warehouse DestinationWarehouse { get; init; }
    public required ProductTable ProductTable { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }
    public required List<RegisterBalance> InventoryEntries { get; init; }

    public static WarehouseTransferLifecycleFixture Create()
    {
        var data = WarehouseTransferTestData.CreateBaseData();
        var userContext = new FakeUserContext();
        var unitOfWork = new FakeUnitOfWork();
        var postingLock = new FakePostingLock();
        var periodValidator = new FakeAccountingPeriodValidator();
        var postingBatches = new List<PostingBatch>();
        var inventoryEntries = new List<RegisterBalance>();

        var sourceWarehouse = data.Warehouses.Single(x => x.Id == 1);
        var destinationWarehouse = data.Warehouses.Single(x => x.Id == 2);
        var product = data.Products.Single();
        var productTable = data.ProductTables.Single();

        var doc = WarehouseTransferTestData.BuildDoc(
            id: 100,
            sourceWarehouse: sourceWarehouse,
            destinationWarehouse: destinationWarehouse,
            product: product,
            productTable: productTable);
        data.Docs.Add(doc);

        var inventoryCommand = new FakeCommandRepository<RegisterBalance>(inventoryEntries, entity => entity.Id = entity.Id == 0 ? inventoryEntries.Count + 1 : entity.Id);
        var postingBatchCommand = new FakeCommandRepository<PostingBatch>(postingBatches, entity => entity.Id = entity.Id == 0 ? postingBatches.Count + 1 : entity.Id);

        var inventoryDispatcher = new InventoryDispatcher(
            new NoopInventoryHandler<PurchaseDoc>(),
            new NoopInventoryHandler<SaleDoc>(),
            new WarehouseTransferInventoryHandler(),
            new NoopInventoryHandler<InventoryAdjustmentDoc>(),
            inventoryCommand);

        var lifecycleService = new WarehouseTransferLifecycleService(
            userContext,
            new FakeQueryBuilder(),
            postingLock,
            periodValidator,
            new FakeActiveInventoryCountGuardService(),
            new FakeAuditLogService(),
            inventoryDispatcher,
            new FakeQueryRepository<WarehouseTransferDoc>(data.Docs),
            new FakeCommandRepository<WarehouseTransferDoc>(data.Docs),
            new FakeCommandRepository<ProductTable>(data.ProductTables),
            new FakeQueryRepository<PostingBatch>(postingBatches),
            postingBatchCommand,
            new FakeQueryRepository<RegisterBalance>(inventoryEntries),
            inventoryCommand,
            NullLogger<WarehouseTransferLifecycleService>.Instance,
            unitOfWork);

        return new WarehouseTransferLifecycleFixture
        {
            LifecycleService = lifecycleService,
            PostingLock = postingLock,
            PeriodValidator = periodValidator,
            Doc = doc,
            SourceWarehouse = sourceWarehouse,
            DestinationWarehouse = destinationWarehouse,
            ProductTable = productTable,
            PostingBatches = postingBatches,
            InventoryEntries = inventoryEntries
        };
    }
}

file static class WarehouseTransferTestData
{
    public static WarehouseTransferCreateDto BuildCreateDto() => new()
    {
        DocDate = DateTime.Today,
        SourceWarehouseId = 1,
        DestinationWarehouseId = 2,
        Comment = "draft",
        Lines =
        [
            new WarehouseTransferLineRequestDto
            {
                ProductId = 100,
                UnitId = 1,
                Quantity = 1,
                Comment = "line",
                Items =
                [
                    new WarehouseTransferTableRequestDto
                    {
                        ProductTableId = 500,
                        CostPrice = 25m
                    }
                ]
            }
        ]
    };

    public static WarehouseTransferUpdateDto BuildUpdateDto() => new()
    {
        DocDate = DateTime.Today,
        SourceWarehouseId = 1,
        DestinationWarehouseId = 2,
        Comment = "updated",
        StateId = StateIdConst.ACTIVE,
        Lines = BuildCreateDto().Lines
    };

    public static (List<WarehouseTransferDoc> Docs, List<Organization> Organizations, List<Warehouse> Warehouses, List<Product> Products, List<Unit> Units, List<ProductTable> ProductTables) CreateBaseData()
    {
        var docs = new List<WarehouseTransferDoc>();
        var organizations = new List<Organization>
        {
            new() { Id = 8, ShortName = "Org", FullName = "Org", Inn = "123", RegionId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today, SetupStatus = "DONE" }
        };
        var warehouses = new List<Warehouse>
        {
            new() { Id = 1, OrganizationId = 8, Name = "Source", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today, IsMain = true },
            new() { Id = 2, OrganizationId = 8, Name = "Destination", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today, IsMain = false }
        };
        var products = new List<Product>
        {
            new() { Id = 100, OrganizationId = 8, Name = "Product", UnitId = 1, IsService = false, IsPieceTracked = true, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var units = new List<Unit>
        {
            new() { Id = 1, Code = "pcs", Name = "pcs", StateId = StateIdConst.ACTIVE }
        };
        var productTables = new List<ProductTable>
        {
            new() { Id = 500, ProductId = 100, OrganizationId = 8, CurrentWarehouseId = 1, StateId = StateIdConst.ACTIVE, StatusId = ProductTableStatusIdConst.IN_STOCK, CreatedDate = DateTime.Today, MarkingNumber = "M1" }
        };

        return (docs, organizations, warehouses, products, units, productTables);
    }

    public static WarehouseTransferDoc BuildDoc(
        long id = 10,
        short statusId = DocumentStatusIdConst.DRAFT,
        Warehouse? sourceWarehouse = null,
        Warehouse? destinationWarehouse = null,
        Product? product = null,
        ProductTable? productTable = null)
    {
        sourceWarehouse ??= new Warehouse { Id = 1, OrganizationId = 8, Name = "Source", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today };
        destinationWarehouse ??= new Warehouse { Id = 2, OrganizationId = 8, Name = "Destination", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today };
        product ??= new Product { Id = 100, OrganizationId = 8, Name = "Product", UnitId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today };
        productTable ??= new ProductTable { Id = 500, ProductId = 100, OrganizationId = 8, CurrentWarehouseId = 1, StateId = StateIdConst.ACTIVE, StatusId = ProductTableStatusIdConst.IN_STOCK, CreatedDate = DateTime.Today };

        var table = new WarehouseTransferDocTable
        {
            Id = 1000,
            ProductTableId = productTable.Id,
            ProductTable = productTable,
            SourceWarehouseId = sourceWarehouse.Id,
            DestinationWarehouseId = destinationWarehouse.Id,
            CostPrice = 25m
        };

        var line = new WarehouseTransferLine
        {
            Id = 100,
            ProductId = product.Id,
            Product = product,
            UnitId = 1,
            Unit = new Unit { Id = 1, Code = "pcs", Name = "pcs", StateId = StateIdConst.ACTIVE },
            Quantity = 1,
            WarehouseTransferDocTables = [table]
        };
        table.Owner = line;
        table.OwnerId = line.Id;

        var doc = new WarehouseTransferDoc
        {
            Id = id,
            OrganizationId = 8,
            DocNumber = $"WTR-{id}",
            DocDate = DateTime.Today,
            SourceWarehouseId = sourceWarehouse.Id,
            DestinationWarehouseId = destinationWarehouse.Id,
            StatusId = statusId,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today,
            SourceWarehouse = sourceWarehouse,
            DestinationWarehouse = destinationWarehouse,
            WarehouseTransferLines = [line]
        };
        line.Owner = doc;
        line.OwnerId = doc.Id;

        return doc;
    }
}

file sealed class FakeUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class FakeUnitOfWork : IUnitOfWork
{
    public int BeginCount { get; private set; }
    public int CommitCount { get; private set; }
    public int RollbackCount { get; private set; }

    public Task BeginAsync(CancellationToken ct = default)
    {
        BeginCount++;
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        CommitCount++;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        RollbackCount++;
        return Task.CompletedTask;
    }
}

file sealed class FakeDocNumberGenerator : IDocNumberGenerator
{
    public Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-000001");
}

file sealed class FakeWarehouseTransferLifecycleService : IWarehouseTransferLifecycleService
{
    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class FakePostingLock : IDocumentPostingLock
{
    public int AcquireCount { get; private set; }

    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default)
    {
        AcquireCount++;
        return Task.CompletedTask;
    }

    public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) =>
        Task.FromResult(true);
}

file sealed class FakeActiveInventoryCountGuardService : IActiveInventoryCountGuardService
{
    public Task<Result> EnsureWarehouseIsNotBlockedAsync(int organizationId, int warehouseId, string operationName, long? currentInventoryCountId = null, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

file sealed class FakeAccountingPeriodValidator : IAccountingPeriodValidator
{
    public bool IsOpen { get; set; } = true;

    public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) =>
        Task.FromResult(IsOpen
            ? Result.Success()
            : Result.Failure(Error.Business("AccountingPeriod.Closed", $"Accounting period {date:yyyy-MM} is closed.")));
}

file sealed class FakeAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class FakeCommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
{
    private readonly Action<TEntity>? _onCreate;
    private readonly List<TEntity>? _store;

    public FakeCommandRepository(List<TEntity>? store = null, Action<TEntity>? onCreate = null)
    {
        _store = store;
        _onCreate = onCreate;
    }

    public List<TEntity> CreatedEntities { get; } = new();
    public List<TEntity> UpdatedEntities { get; } = new();
    public int DeletePredicateCount { get; private set; }

    public Task CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        _onCreate?.Invoke(entity);
        CreatedEntities.Add(entity);
        _store?.Add(entity);
        return Task.CompletedTask;
    }

    public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            _onCreate?.Invoke(entity);
            CreatedEntities.Add(entity);
            _store?.Add(entity);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        UpdatedEntities.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        UpdatedEntities.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        _store?.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        if (_store != null)
        {
            foreach (var entity in entities.ToList())
                _store.Remove(entity);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        DeletePredicateCount++;
        if (_store != null)
        {
            var compiled = predicate.Compile();
            _store.RemoveAll(x => compiled(x));
        }

        return Task.CompletedTask;
    }

    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public FakeQueryRepository(List<TEntity> data)
    {
        _data = data;
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Any(predicate));

    public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).FirstOrDefault());

    public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).Select(specification.Selector).FirstOrDefault());

    public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).ToList());

    public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).Select(specification.Selector).ToList());

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default)
    {
        var take = specification.Take.GetValueOrDefault(50);
        var items = _data.AsQueryable().Where(specification.Criteria).Skip(specification.Skip).Take(take).ToList();
        var total = _data.AsQueryable().Count(specification.Criteria);
        return Task.FromResult(new PagedList<TEntity>(items, total));
    }

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
    {
        var filtered = _data.AsQueryable().Where(specification.Criteria).Select(specification.Selector);
        var take = specification.Take.GetValueOrDefault(50);
        var items = filtered.Where(specification.ResultCriteria).Skip(specification.Skip).Take(take).ToList();
        var total = filtered.Count(specification.ResultCriteria);
        return Task.FromResult(new PagedList<TResult>(items, total));
    }
}

file sealed class FakeQueryBuilder : IQueryBuilder
{
    private static readonly FakeQueryBuilderResolver Resolver = new();

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

file sealed class FakeQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;

    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new FakeProjectionBuilder<TEntity, TResult>();
}

file sealed class FakeProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}

file sealed class NoopInventoryHandler<T> : IInventoryDocumentHandler<T>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(T document, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<RegisterBalance>()));
}
