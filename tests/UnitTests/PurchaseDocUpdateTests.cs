using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.Contracts;
using Application.Features.CounterpartyCards;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.InventoryCounts;
using Application.Features.InventoryRegisterBalances;
using Application.Features.PurchaseDocs;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.Warehouses;
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

public class PurchaseDocUpdateTests
{
    [Fact]
    public async Task Update_ShouldPreserveExistingState_WhenDtoStateIdIsDefault()
    {
        var fixture = PurchaseDocFixture.Create();

        var result = await fixture.Service.UpdateAsync(fixture.Doc.Id, fixture.BuildUpdateDto(stateId: 0, contractId: 9));

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.DocCommand.UpdatedEntities);
        Assert.Equal(StateIdConst.ACTIVE, fixture.Doc.StateId);
        Assert.Equal(9, fixture.Doc.ContractId);
        Assert.Equal(1, fixture.ProductLineCommand.DeletePredicateCount);
        Assert.Equal(1, fixture.TableLineCommand.DeletePredicateCount);
        Assert.Single(fixture.ProductLineCommand.CreatedEntities);
    }

    [Fact]
    public async Task Update_ShouldRejectUnknownUnit_WithBusinessError()
    {
        var fixture = PurchaseDocFixture.Create();
        var dto = fixture.BuildUpdateDto(stateId: 0, contractId: 9);
        dto.Lines[0].UnitId = 999;

        var result = await fixture.Service.UpdateAsync(fixture.Doc.Id, dto);

        Assert.False(result.IsSuccess);
        Assert.Equal("PurchaseDoc.UnitNotFound", result.Error.Code);
        Assert.Empty(fixture.DocCommand.UpdatedEntities);
    }

    [Fact]
    public async Task Update_ShouldDeleteTables_ByScalarOwnerIdPredicate()
    {
        var fixture = PurchaseDocFixture.Create(rejectNavigationBulkDelete: true);

        var result = await fixture.Service.UpdateAsync(fixture.Doc.Id, fixture.BuildUpdateDto(stateId: 0, contractId: 9));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, fixture.TableLineCommand.DeletePredicateCount);
    }
}

public class PurchaseLifecycleRegressionTests
{
    [Fact]
    public async Task Cancel_Draft_ShouldDeleteTables_ByScalarOwnerIdPredicate()
    {
        var fixture = PurchaseLifecycleFixture.Create(rejectNavigationBulkDelete: true);

        var result = await fixture.Service.CancelAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.CANCELLED, fixture.Doc.StatusId);
        Assert.Equal(1, fixture.TableCommand.DeletePredicateCount);
    }

    [Fact]
    public async Task Confirm_ShouldBeIdempotent_WhenAlreadyPosted()
    {
        var fixture = PurchaseLifecycleFixture.Create();
        fixture.Doc.StatusId = DocumentStatusIdConst.POSTED;
        fixture.PostingBatches.Add(new PostingBatch
        {
            Id = 1,
            OrganizationId = fixture.Doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.PURCHASE,
            DocumentId = fixture.Doc.Id,
            Status = PostingBatchStatusConst.POSTED,
            PostedAt = DateTime.Now
        });

        var result = await fixture.Service.ConfirmAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, fixture.AccountingDispatcher.CallCount);
        Assert.Equal(0, fixture.InventoryDispatcher.CallCount);
        Assert.Single(fixture.PostingBatches);
    }
}

file sealed class PurchaseDocFixture
{
    public required PurchaseDocService Service { get; init; }
    public required PurchaseDoc Doc { get; init; }
    public required PurchaseDocCommandRepository<PurchaseDoc> DocCommand { get; init; }
    public required PurchaseDocCommandRepository<PurchaseDocProduct> ProductLineCommand { get; init; }
    public required PurchaseDocCommandRepository<PurchaseDocTable> TableLineCommand { get; init; }

    public static PurchaseDocFixture Create(bool rejectNavigationBulkDelete = false)
    {
        var docs = new List<PurchaseDoc>();
        var productTables = new List<ProductTable>();
        var purchaseTables = new List<PurchaseDocTable>();
        var purchaseProducts = new List<PurchaseDocProduct>();
        var contracts = new List<Contract>
        {
            new() { Id = 9, CounterpartyId = 18, ContractTypeId = 1, ContractDate = DateTime.Today, StartDate = DateTime.Today, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            new() { Id = 10, CounterpartyId = 18, ContractTypeId = 1, ContractDate = DateTime.Today, StartDate = DateTime.Today, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var counterparties = new List<CounterpartyCard>
        {
            new() { Id = 18, OrganizationId = 8, CounterpartyTypeId = 1, ShortName = "CP", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var warehouses = new List<Warehouse>
        {
            new() { Id = 7, OrganizationId = 8, Name = "Main", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today, IsMain = true }
        };
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "UZS", Name = "Uzbek Sum", StateId = StateIdConst.ACTIVE }
        };
        var units = new List<Unit>
        {
            new() { Id = 1, Code = "pcs", Name = "pcs", StateId = StateIdConst.ACTIVE }
        };
        var vatRates = new List<VatRate>
        {
            new() { Id = 2, Code = "VAT12", Name = "VAT 12", Rate = 12, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var products = new List<Product>
        {
            new() { Id = 24, OrganizationId = 8, Name = "Tracked product", UnitId = 1, IsService = false, IsPieceTracked = true, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };

        var existingProductTable = new ProductTable
        {
            Id = 501,
            ProductId = 24,
            OrganizationId = 8,
            MarkingNumber = "old-mark",
            StateId = StateIdConst.ACTIVE,
            StatusId = ProductTableStatusIdConst.RESERVED,
            CreatedDate = DateTime.Today
        };
        productTables.Add(existingProductTable);

        var existingTable = new PurchaseDocTable
        {
            Id = 701,
            ProductTableId = existingProductTable.Id,
            ProductTable = existingProductTable,
            Amount = 5000,
            VatRateId = 2,
            VatAmount = 600,
            TotalAmount = 5600
        };
        purchaseTables.Add(existingTable);

        var existingLine = new PurchaseDocProduct
        {
            Id = 601,
            ProductId = 24,
            Product = products[0],
            UnitId = 1,
            Unit = units[0],
            Quantity = 1,
            UnitPrice = 5000,
            Amount = 5000,
            VatRateId = 2,
            VatAmount = 600,
            TotalAmount = 5600,
            PurchaseDocTables = [existingTable]
        };
        existingTable.Owner = existingLine;
        existingTable.OwnerId = existingLine.Id;
        purchaseProducts.Add(existingLine);

        var doc = new PurchaseDoc
        {
            Id = 100,
            OrganizationId = 8,
            DocNumber = "PUR-100",
            DocDate = DateTime.Today,
            CounterpartyId = 18,
            WarehouseId = 7,
            CurrencyId = 1,
            ExchangeRate = 1,
            TotalAmount = 5000,
            VatAmount = 600,
            FinalAmount = 5600,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today,
            ContractId = 10,
            PurchaseDocProducts = [existingLine]
        };
        existingLine.Owner = doc;
        existingLine.OwnerId = doc.Id;
        docs.Add(doc);

        var docCommand = new PurchaseDocCommandRepository<PurchaseDoc>(docs);
        var productLineCommand = new PurchaseDocCommandRepository<PurchaseDocProduct>(purchaseProducts, entity => entity.Id = entity.Id == 0 ? purchaseProducts.Count + 1000 : entity.Id);
        var tableLineCommand = new PurchaseDocCommandRepository<PurchaseDocTable>(
            purchaseTables,
            entity => entity.Id = entity.Id == 0 ? purchaseTables.Count + 2000 : entity.Id,
            rejectNavigationBulkDelete);
        var productTableCommand = new PurchaseDocCommandRepository<ProductTable>(productTables, entity => entity.Id = entity.Id == 0 ? productTables.Count + 3000 : entity.Id);

        var service = new PurchaseDocService(
            new PurchaseDocUserContext(),
            new PurchaseDocQueryBuilder(),
            new PurchaseDocLifecycleService(),
            new PurchaseDocAuditLogService(),
            new PurchaseDocDocNumberGenerator(),
            new PurchaseDocQueryRepository<PurchaseDoc>(docs),
            new PurchaseDocQueryRepository<VatRate>(vatRates),
            new PurchaseDocQueryRepository<Contract>(contracts),
            new PurchaseDocQueryRepository<CounterpartyCard>(counterparties),
            new PurchaseDocQueryRepository<Warehouse>(warehouses),
            new PurchaseDocQueryRepository<Currency>(currencies),
            new PurchaseDocQueryRepository<Unit>(units),
            new PurchaseDocQueryRepository<Product>(products),
            productTableCommand,
            new PurchaseDocQueryRepository<PurchaseDocTable>(purchaseTables),
            docCommand,
            productLineCommand,
            tableLineCommand,
            NullLogger<PurchaseDocService>.Instance,
            new PurchaseDocUnitOfWork());

        return new PurchaseDocFixture
        {
            Service = service,
            Doc = doc,
            DocCommand = docCommand,
            ProductLineCommand = productLineCommand,
            TableLineCommand = tableLineCommand
        };
    }

    public PurchaseDocUpdateDto BuildUpdateDto(short stateId, long? contractId) => new()
    {
        DocDate = new DateTime(2026, 7, 3, 15, 59, 16),
        CounterpartyId = 18,
        WarehouseId = 7,
        CurrencyId = 1,
        ExchangeRate = 1,
        ContractId = contractId,
        Comment = null,
        StateId = stateId,
        Lines =
        [
            new PurchaseDocLineDto
            {
                ProductId = 24,
                Quantity = 1,
                UnitId = 1,
                UnitPrice = 10000,
                VatRateId = 2,
                Items =
                [
                    new PurchaseDocLineItemDto
                    {
                        MarkingNumber = "qa1qq1q1",
                        SerialNumber = null
                    }
                ]
            }
        ]
    };
}

file sealed class PurchaseLifecycleFixture
{
    public required PurchaseLifecycleService Service { get; init; }
    public required PurchaseDoc Doc { get; init; }
    public required PurchaseDocCommandRepository<PurchaseDocTable> TableCommand { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }
    public required PurchaseLifecycleAccountingDispatcher AccountingDispatcher { get; init; }
    public required PurchaseLifecycleInventoryDispatcher InventoryDispatcher { get; init; }

    public static PurchaseLifecycleFixture Create(bool rejectNavigationBulkDelete = false)
    {
        var docs = new List<PurchaseDoc>();
        var productTables = new List<ProductTable>();
        var purchaseTables = new List<PurchaseDocTable>();
        var purchaseProducts = new List<PurchaseDocProduct>();
        var postingBatches = new List<PostingBatch>();
        var accountingEntries = new List<AccountingRegisterEntry>();
        var inventoryEntries = new List<RegisterBalance>();
        var counterpartyEntries = new List<CounterpartyRegisterBalance>();
        var productPrices = new List<ProductPrice>();
        var saleConditions = new List<SaleCondition>();

        var product = new Product
        {
            Id = 24,
            OrganizationId = 8,
            Name = "Tracked product",
            UnitId = 1,
            IsService = false,
            IsPieceTracked = true,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        };

        var existingProductTable = new ProductTable
        {
            Id = 501,
            ProductId = 24,
            OrganizationId = 8,
            MarkingNumber = "old-mark",
            StateId = StateIdConst.ACTIVE,
            StatusId = ProductTableStatusIdConst.RESERVED,
            CreatedDate = DateTime.Today
        };
        productTables.Add(existingProductTable);

        var existingTable = new PurchaseDocTable
        {
            Id = 701,
            ProductTableId = existingProductTable.Id,
            ProductTable = existingProductTable,
            Amount = 5000,
            VatRateId = 2,
            VatAmount = 600,
            TotalAmount = 5600
        };
        purchaseTables.Add(existingTable);

        var existingLine = new PurchaseDocProduct
        {
            Id = 601,
            ProductId = 24,
            Product = product,
            UnitId = 1,
            Unit = new Unit { Id = 1, Code = "pcs", Name = "pcs", StateId = StateIdConst.ACTIVE },
            Quantity = 1,
            UnitPrice = 5000,
            Amount = 5000,
            VatRateId = 2,
            VatAmount = 600,
            TotalAmount = 5600,
            PurchaseDocTables = [existingTable]
        };
        existingTable.Owner = existingLine;
        existingTable.OwnerId = existingLine.Id;
        purchaseProducts.Add(existingLine);

        var doc = new PurchaseDoc
        {
            Id = 100,
            OrganizationId = 8,
            DocNumber = "PUR-100",
            DocDate = DateTime.Today,
            CounterpartyId = 18,
            WarehouseId = 7,
            CurrencyId = 1,
            ExchangeRate = 1,
            TotalAmount = 5000,
            VatAmount = 600,
            FinalAmount = 5600,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today,
            ContractId = 10,
            PurchaseDocProducts = [existingLine]
        };
        existingLine.Owner = doc;
        existingLine.OwnerId = doc.Id;
        docs.Add(doc);

        var tableCommand = new PurchaseDocCommandRepository<PurchaseDocTable>(
            purchaseTables,
            entity => entity.Id = entity.Id == 0 ? purchaseTables.Count + 2000 : entity.Id,
            rejectNavigationBulkDelete);

        var accountingDispatcher = new PurchaseLifecycleAccountingDispatcher();
        var inventoryDispatcher = new PurchaseLifecycleInventoryDispatcher();

        var service = new PurchaseLifecycleService(
            new PurchaseDocUserContext(),
            new PurchaseDocQueryBuilder(),
            new PurchaseLifecyclePostingLock(),
            new PurchaseLifecyclePeriodValidator(),
            new PurchaseDocAuditLogService(),
            accountingDispatcher,
            inventoryDispatcher,
            new PurchaseLifecycleInventoryCountGuardService(),
            new PurchaseLifecycleCounterpartyRegisterService(),
            new PurchaseDocQueryRepository<PurchaseDoc>(docs),
            new PurchaseDocCommandRepository<PurchaseDoc>(docs),
            new PurchaseDocQueryRepository<ProductTable>(productTables),
            new PurchaseDocCommandRepository<ProductTable>(productTables),
            new PurchaseDocQueryRepository<PurchaseDocTable>(purchaseTables),
            tableCommand,
            new PurchaseDocQueryRepository<ProductPrice>(productPrices),
            new PurchaseDocCommandRepository<ProductPrice>(productPrices),
            new PurchaseDocQueryRepository<PostingBatch>(postingBatches),
            new PurchaseDocCommandRepository<PostingBatch>(postingBatches, entity => entity.Id = entity.Id == 0 ? postingBatches.Count + 1 : entity.Id),
            new PurchaseDocQueryRepository<AccountingRegisterEntry>(accountingEntries),
            new PurchaseDocCommandRepository<AccountingRegisterEntry>(accountingEntries, entity => entity.Id = entity.Id == 0 ? accountingEntries.Count + 1 : entity.Id),
            new PurchaseDocQueryRepository<RegisterBalance>(inventoryEntries),
            new PurchaseDocCommandRepository<RegisterBalance>(inventoryEntries, entity => entity.Id = entity.Id == 0 ? inventoryEntries.Count + 1 : entity.Id),
            new PurchaseDocQueryRepository<CounterpartyRegisterBalance>(counterpartyEntries),
            new PurchaseDocQueryRepository<SaleCondition>(saleConditions),
            NullLogger<PurchaseLifecycleService>.Instance,
            new PurchaseDocUnitOfWork());

        return new PurchaseLifecycleFixture
        {
            Service = service,
            Doc = doc,
            TableCommand = tableCommand,
            PostingBatches = postingBatches,
            AccountingDispatcher = accountingDispatcher,
            InventoryDispatcher = inventoryDispatcher
        };
    }
}

file sealed class PurchaseDocUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class PurchaseDocUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class PurchaseDocDocNumberGenerator : IDocNumberGenerator
{
    public Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-000001");
}

file sealed class PurchaseDocLifecycleService : IPurchaseLifecycleService
{
    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class PurchaseLifecyclePostingLock : IDocumentPostingLock
{
    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.FromResult(true);
}

file sealed class PurchaseLifecyclePeriodValidator : IAccountingPeriodValidator
{
    public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class PurchaseLifecycleInventoryCountGuardService : IActiveInventoryCountGuardService
{
    public Task<Result> EnsureWarehouseIsNotBlockedAsync(int organizationId, int warehouseId, string operationName, long? currentInventoryCountId = null, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

file sealed class PurchaseLifecycleAccountingDispatcher : IAccountingDispatcher
{
    public int CallCount { get; private set; }

    public Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null)
    {
        CallCount++;
        return Task.FromResult(Result.Success(new List<AccountingRegisterEntry>()));
    }
}

file sealed class PurchaseLifecycleInventoryDispatcher : IInventoryDispatcher
{
    public int CallCount { get; private set; }

    public Task<Result<List<RegisterBalance>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null)
    {
        CallCount++;
        return Task.FromResult(Result.Success(new List<RegisterBalance>()));
    }
}

file sealed class PurchaseLifecycleCounterpartyRegisterService : IPurchaseCounterpartyRegisterService
{
    public Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(PurchaseDoc purchase, long postingBatchId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<CounterpartyRegisterBalance>()));

    public Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(PurchaseDoc purchase, long postingBatchId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<CounterpartyRegisterBalance>()));
}

file sealed class PurchaseDocAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class PurchaseDocCommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity>? _store;
    private readonly Action<TEntity>? _onCreate;
    private readonly bool _rejectNavigationBulkDelete;

    public PurchaseDocCommandRepository(
        List<TEntity>? store = null,
        Action<TEntity>? onCreate = null,
        bool rejectNavigationBulkDelete = false)
    {
        _store = store;
        _onCreate = onCreate;
        _rejectNavigationBulkDelete = rejectNavigationBulkDelete;
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
        if (_rejectNavigationBulkDelete && predicate.Body.ToString().Contains(".Owner.OwnerId"))
            throw new InvalidOperationException("ExecuteDeleteAsync cannot translate navigation-based predicate for this repository.");

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

file sealed class PurchaseDocQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public PurchaseDocQueryRepository(List<TEntity> data)
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

file sealed class PurchaseDocQueryBuilder : IQueryBuilder
{
    private static readonly PurchaseDocQueryBuilderResolver Resolver = new();

    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = Resolver });

    public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true };

    public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default! };

    public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : SharedKernel.Filters.IPaginationFilter =>
        new() { Criteria = _ => true, Skip = 0, Take = 50 };

    public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : SharedKernel.Filters.IPaginationFilter =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default!, Skip = 0, Take = 50 };
}

file sealed class PurchaseDocQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new PurchaseDocProjectionBuilder<TEntity, TResult>();
}

file sealed class PurchaseDocProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
