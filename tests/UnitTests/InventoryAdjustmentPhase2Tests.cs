using Application.Abstractions;
using Application.Abstractions.Authentication;
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

public class InventoryAdjustmentPhase2Tests
{
    [Fact]
    public async Task Confirm_PositiveAdjustment_ShouldCreateProductTableAndInMovement()
    {
        var fixture = InventoryAdjustmentLifecycleFixture.CreatePositive();

        var result = await fixture.Service.ConfirmAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.Doc.StatusId);
        var createdProductTable = fixture.ProductTables.Single(x => x.Id != 500);
        Assert.Equal(fixture.Doc.WarehouseId, createdProductTable.CurrentWarehouseId);
        Assert.Equal(ProductTableStatusIdConst.IN_STOCK, createdProductTable.StatusId);
        Assert.Single(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.POSTED);
        var movement = Assert.Single(fixture.RegisterBalances, x => x.ReversalEntryId == null);
        Assert.Equal(OperationTypeIdConst.IN, movement.OperationTypeId);
        Assert.Equal(createdProductTable.Id, movement.ProductTableId);
    }

    [Fact]
    public async Task Confirm_WriteOff_ShouldMutateExistingProductTableAndCreateOutMovement()
    {
        var fixture = InventoryAdjustmentLifecycleFixture.CreateWriteOff();

        var result = await fixture.Service.ConfirmAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.Doc.StatusId);
        Assert.Equal(ProductTableStatusIdConst.WRITTEN_OFF, fixture.ExistingProductTable.StatusId);
        Assert.Equal(StateIdConst.PASSIVE, fixture.ExistingProductTable.StateId);
        var movement = Assert.Single(fixture.RegisterBalances, x => x.ReversalEntryId == null);
        Assert.Equal(OperationTypeIdConst.OUT, movement.OperationTypeId);
        Assert.Equal(fixture.ExistingProductTable.Id, movement.ProductTableId);
    }

    [Fact]
    public async Task Cancel_AfterPositiveConfirm_ShouldReverseMovementAndDeactivateCreatedProductTable()
    {
        var fixture = InventoryAdjustmentLifecycleFixture.CreatePositive();
        await fixture.Service.ConfirmAsync(fixture.Doc.Id);

        var result = await fixture.Service.CancelAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.CANCELLED, fixture.Doc.StatusId);
        Assert.Equal(2, fixture.PostingBatches.Count);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSED);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSAL);
        Assert.Equal(2, fixture.RegisterBalances.Count);
        var createdProductTable = fixture.ProductTables.Single(x => x.Id != 500);
        Assert.Null(createdProductTable.CurrentWarehouseId);
        Assert.Equal(ProductTableStatusIdConst.BLOCKED, createdProductTable.StatusId);
        Assert.Equal(StateIdConst.PASSIVE, createdProductTable.StateId);
    }

    [Fact]
    public async Task Confirm_Twice_ShouldBeIdempotent()
    {
        var fixture = InventoryAdjustmentLifecycleFixture.CreateWriteOff();

        var first = await fixture.Service.ConfirmAsync(fixture.Doc.Id);
        var second = await fixture.Service.ConfirmAsync(fixture.Doc.Id);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Single(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.POSTED);
        Assert.Single(fixture.RegisterBalances, x => x.ReversalEntryId == null);
    }

    [Fact]
    public async Task Confirm_ShouldFailWhenPeriodClosed()
    {
        var fixture = InventoryAdjustmentLifecycleFixture.CreateWriteOff();
        fixture.PeriodValidator.AlwaysClosed = true;

        var result = await fixture.Service.ConfirmAsync(fixture.Doc.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPeriod.Closed", result.Error.Code);
        Assert.Empty(fixture.PostingBatches);
        Assert.Empty(fixture.RegisterBalances);
    }
}

file sealed class InventoryAdjustmentLifecycleFixture
{
    public required InventoryAdjustmentLifecycleService Service { get; init; }
    public required InventoryAdjustmentDoc Doc { get; init; }
    public required ProductTable ExistingProductTable { get; init; }
    public required List<ProductTable> ProductTables { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }
    public required List<RegisterBalance> RegisterBalances { get; init; }
    public required FakeLifecyclePeriodValidator PeriodValidator { get; init; }

    public static InventoryAdjustmentLifecycleFixture CreatePositive()
    {
        var context = BuildContext("POSITIVE_ADJUSTMENT", includeExistingTableReference: false);
        return Create(context);
    }

    public static InventoryAdjustmentLifecycleFixture CreateWriteOff()
    {
        var context = BuildContext("WRITE_OFF", includeExistingTableReference: true);
        return Create(context);
    }

    private static InventoryAdjustmentLifecycleFixture Create(InventoryAdjustmentLifecycleContext context)
    {
        var periodValidator = new FakeLifecyclePeriodValidator();
        var postingBatchCommand = new FakeLifecycleCommandRepository<PostingBatch>(context.PostingBatches, entity => entity.Id = entity.Id == 0 ? context.NextPostingBatchId++ : entity.Id);
        var registerCommand = new FakeLifecycleCommandRepository<RegisterBalance>(context.RegisterBalances, entity => entity.Id = entity.Id == 0 ? context.NextRegisterBalanceId++ : entity.Id);
        var productTableCommand = new FakeLifecycleCommandRepository<ProductTable>(context.ProductTables, entity => entity.Id = entity.Id == 0 ? context.NextProductTableId++ : entity.Id);

        var inventoryDispatcher = new InventoryDispatcher(
            new NoopInventoryDocumentHandler<PurchaseDoc>(),
            new NoopInventoryDocumentHandler<SaleDoc>(),
            new NoopInventoryDocumentHandler<WarehouseTransferDoc>(),
            new InventoryAdjustmentInventoryHandler(),
            registerCommand);

        var service = new InventoryAdjustmentLifecycleService(
            new FakeLifecycleUserContext(),
            new FakeLifecycleQueryBuilder(),
            new FakeLifecycleDocumentPostingLock(),
            periodValidator,
            new FakeLifecycleActiveInventoryCountGuardService(),
            new FakeLifecycleAuditLogService(),
            inventoryDispatcher,
            new FakeLifecycleQueryRepository<InventoryAdjustmentDoc>(context.Docs),
            new FakeLifecycleCommandRepository<InventoryAdjustmentDoc>(context.Docs),
            new FakeLifecycleCommandRepository<InventoryAdjustmentDocTable>(context.Tables),
            productTableCommand,
            new FakeLifecycleQueryRepository<PostingBatch>(context.PostingBatches),
            postingBatchCommand,
            new FakeLifecycleQueryRepository<RegisterBalance>(context.RegisterBalances),
            registerCommand,
            NullLogger<InventoryAdjustmentLifecycleService>.Instance,
            new FakeLifecycleUnitOfWork());

        return new InventoryAdjustmentLifecycleFixture
        {
            Service = service,
            Doc = context.Doc,
            ExistingProductTable = context.ExistingProductTable,
            ProductTables = context.ProductTables,
            PostingBatches = context.PostingBatches,
            RegisterBalances = context.RegisterBalances,
            PeriodValidator = periodValidator
        };
    }

    private static InventoryAdjustmentLifecycleContext BuildContext(string adjustmentType, bool includeExistingTableReference)
    {
        var warehouse = new Warehouse
        {
            Id = 1,
            OrganizationId = 8,
            Name = "Main",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        };

        var product = new Product
        {
            Id = 100,
            OrganizationId = 8,
            Name = "Product",
            UnitId = 1,
            IsService = false,
            IsPieceTracked = true,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        };

        var existingProductTable = new ProductTable
        {
            Id = 500,
            ProductId = 100,
            OrganizationId = 8,
            CurrentWarehouseId = 1,
            StatusId = ProductTableStatusIdConst.IN_STOCK,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        };

        var table = new InventoryAdjustmentDocTable
        {
            Id = 900,
            ProductTableId = includeExistingTableReference ? existingProductTable.Id : null,
            ProductTable = includeExistingTableReference ? existingProductTable : null,
            CostPrice = 20m
        };

        var line = new InventoryAdjustmentLine
        {
            Id = 800,
            ProductId = product.Id,
            Product = product,
            UnitId = 1,
            Quantity = 1,
            InventoryAdjustmentDocTables = [table]
        };

        var doc = new InventoryAdjustmentDoc
        {
            Id = 700,
            OrganizationId = 8,
            DocNumber = "IAD-1",
            DocDate = DateTime.Today,
            WarehouseId = warehouse.Id,
            Warehouse = warehouse,
            AdjustmentType = adjustmentType,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today,
            InventoryAdjustmentLines = [line]
        };

        line.OwnerId = doc.Id;
        line.Owner = doc;
        table.OwnerId = line.Id;
        table.Owner = line;

        return new InventoryAdjustmentLifecycleContext
        {
            Doc = doc,
            ExistingProductTable = existingProductTable,
            Docs = [doc],
            Tables = [table],
            ProductTables = includeExistingTableReference ? [existingProductTable] : [existingProductTable],
            PostingBatches = [],
            RegisterBalances = [],
            NextPostingBatchId = 1,
            NextRegisterBalanceId = 1,
            NextProductTableId = 1000
        };
    }
}

file sealed class InventoryAdjustmentLifecycleContext
{
    public required InventoryAdjustmentDoc Doc { get; init; }
    public required ProductTable ExistingProductTable { get; init; }
    public required List<InventoryAdjustmentDoc> Docs { get; init; }
    public required List<InventoryAdjustmentDocTable> Tables { get; init; }
    public required List<ProductTable> ProductTables { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }
    public required List<RegisterBalance> RegisterBalances { get; init; }
    public required long NextPostingBatchId { get; set; }
    public required long NextRegisterBalanceId { get; set; }
    public required int NextProductTableId { get; set; }
}

file sealed class FakeLifecycleUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class FakeLifecycleUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeLifecycleAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class FakeLifecycleDocumentPostingLock : IDocumentPostingLock
{
    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.FromResult(true);
}

file sealed class FakeLifecycleActiveInventoryCountGuardService : IActiveInventoryCountGuardService
{
    public Task<Result> EnsureWarehouseIsNotBlockedAsync(int organizationId, int warehouseId, string operationName, long? currentInventoryCountId = null, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

file sealed class FakeLifecyclePeriodValidator : IAccountingPeriodValidator
{
    public bool AlwaysClosed { get; set; }

    public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) =>
        Task.FromResult(AlwaysClosed
            ? Result.Failure(Error.Business("AccountingPeriod.Closed", $"Accounting period {date:yyyy-MM} is closed."))
            : Result.Success());
}

file sealed class NoopInventoryDocumentHandler<T> : IInventoryDocumentHandler<T>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(T document, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<RegisterBalance>()));
}

file sealed class FakeLifecycleCommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity>? _store;
    private readonly Action<TEntity>? _onCreate;

    public FakeLifecycleCommandRepository(List<TEntity>? store = null, Action<TEntity>? onCreate = null)
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
    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeLifecycleQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public FakeLifecycleQueryRepository(List<TEntity> data)
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
        Task.FromResult(new PagedList<TEntity>([], 0));

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(new PagedList<TResult>([], 0));
}

file sealed class FakeLifecycleQueryBuilder : IQueryBuilder
{
    private static readonly FakeLifecycleQueryBuilderResolver Resolver = new();

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

file sealed class FakeLifecycleQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new FakeLifecycleProjectionBuilder<TEntity, TResult>();
}

file sealed class FakeLifecycleProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
