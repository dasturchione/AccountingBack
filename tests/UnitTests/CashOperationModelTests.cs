using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.CashOperations;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.MoneyRegisterBalances;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.Register.PostingEngines;
using Application.Features.Register.PostingEngines.Builders;
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

public class CashOperationModelTests
{
    [Fact]
    public async Task Create_ShouldPersistPaymentPurposeOnCashOperationHeader()
    {
        var fixture = CashOperationServiceFixture.Create();

        var result = await fixture.Service.CreateAsync(fixture.BuildCreateDto());

        Assert.True(result.IsSuccess);
        var created = fixture.Command.CreatedEntities.Single();
        Assert.Equal(5, created.PaymentPurposeId);
    }

    [Fact]
    public async Task ContextBuilder_ShouldUseHeaderPaymentPurpose()
    {
        var operation = new CashOperation
        {
            Id = 100,
            OrganizationId = 8,
            CashBoxId = 4,
            PaymentPurposeId = 5,
            OperationTypeId = OperationTypeIdConst.OUT,
            PaymentTypeId = 2,
            CounterpartyId = 18,
            DocNumber = "CASH-100",
            DocDate = new DateTime(2026, 7, 3),
            CurrencyId = 1,
            Amount = 700m,
            ExchangeRate = 1m,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        };

        var builder = new CashOperationContextBuilder(
            new CashOperationTestQueryBuilder(),
            new CashOperationTestQueryRepository<PaymentType>(
            [
                new PaymentType { Id = 2, Code = "WIRE", Name = "Wire" }
            ]),
            new CashOperationTestQueryRepository<PaymentPurpose>(
            [
                new PaymentPurpose
                {
                    Id = 5,
                    Code = "SUPPLIER_PAYMENT",
                    Name = "Supplier payment",
                    AliasId = 1,
                    OperationTypeId = OperationTypeIdConst.OUT,
                    RequiresCounterparty = true,
                    Alias = new PostingAlias { Id = 1, Code = AliasConst.Supplier, Name = "Supplier" },
                    OperationType = new OperationType { Id = OperationTypeIdConst.OUT, Code = "OUT", Name = "OUT" }
                }
            ]),
            new CashOperationTestQueryRepository<CounterpartyCard>(
            [
                new CounterpartyCard { Id = 18, ShortName = "Vendor", OrganizationId = 8, CounterpartyTypeId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
            ]),
            new CashOperationPolicyResolver());

        var contexts = await builder.BuildAsync(operation);

        Assert.Single(contexts);
        var context = contexts.Single();
        Assert.Equal(PostingRuleIdConst.CREDIT_OPERATION, context.RuleId);
        Assert.Equal(AliasConst.Supplier, context.RequiredDebitAlias);
        Assert.Equal(AliasConst.PaymentAccount, context.RequiredCreditAlias);
        Assert.Equal("WIRE", context.PaymentMethod);
        Assert.Equal(700m, context.Amounts[AmountSourceConst.Total]);
    }

    [Fact]
    public async Task Confirm_ShouldValidateHeaderPaymentPurpose()
    {
        var fixture = CashLifecycleFixture.Create();

        var result = await fixture.Service.ConfirmAsync(fixture.CashOperation.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.CashOperation.StatusId);
        Assert.Equal(1, fixture.Dispatcher.CallCount);
        Assert.Single(fixture.PostingBatches);
    }

    [Fact]
    public async Task Confirm_ShouldAllowCashCollectionWithoutCounterparty()
    {
        var fixture = CashLifecycleFixture.Create();
        fixture.CashOperation.PaymentPurposeId = 6;
        fixture.CashOperation.PaymentPurpose = new PaymentPurpose
        {
            Id = 6,
            Code = "CASH_COLLECTION_SENT",
            Name = "Cash collection to transit",
            AliasId = 2,
            OperationTypeId = OperationTypeIdConst.OUT,
            RequiresCounterparty = false,
            Alias = new PostingAlias { Id = 2, Code = AliasConst.CashInTransit, Name = "Cash in transit" },
            OperationType = new OperationType { Id = OperationTypeIdConst.OUT, Code = "OUT", Name = "OUT" }
        };
        fixture.CashOperation.CounterpartyId = null;
        fixture.CashOperation.Counterparty = null;

        var result = await fixture.Service.ConfirmAsync(fixture.CashOperation.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.CashOperation.StatusId);
    }
}

file sealed class CashOperationServiceFixture
{
    public required CashOperationService Service { get; init; }
    public required CashOperationCommandRepository<CashOperation> Command { get; init; }

    public static CashOperationServiceFixture Create()
    {
        var docs = new List<CashOperation>();
        var command = new CashOperationCommandRepository<CashOperation>(docs, entity => entity.Id = entity.Id == 0 ? 100 : entity.Id);

        var service = new CashOperationService(
            new CashOperationUserContext(),
            new CashOperationTestQueryBuilder(),
            new CashOperationAuditLogService(),
            new CashOperationLifecycleService(),
            new CashOperationDocNumberGenerator(),
            new CashOperationTestQueryRepository<CashOperation>(docs),
            command,
            NullLogger<CashOperationService>.Instance,
            new CashOperationUnitOfWork());

        return new CashOperationServiceFixture
        {
            Service = service,
            Command = command
        };
    }

    public CashOperationCreateDto BuildCreateDto() => new()
    {
        CashBoxId = 4,
        PaymentPurposeId = 5,
        OperationTypeId = OperationTypeIdConst.OUT,
        PaymentTypeId = 2,
        CounterpartyId = 18,
        DocDate = new DateTime(2026, 7, 3),
        CurrencyId = 1,
        Amount = 700m,
        ExchangeRate = 1m,
        Comment = "cash payment"
    };
}

file sealed class CashLifecycleFixture
{
    public required CashLifecycleService Service { get; init; }
    public required CashOperation CashOperation { get; init; }
    public required CashOperationAccountingDispatcher Dispatcher { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }

    public static CashLifecycleFixture Create()
    {
        var postingBatches = new List<PostingBatch>();
        var accountingEntries = new List<AccountingRegisterEntry>();
        var moneyEntries = new List<MoneyRegisterBalance>();
        var counterpartyEntries = new List<CounterpartyRegisterBalance>();

        var cashOperation = new CashOperation
        {
            Id = 100,
            OrganizationId = 8,
            CashBoxId = 4,
            CashBox = new CashBox { Id = 4, Name = "Main cash", OrganizationId = 8, CurrencyId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            PaymentPurposeId = 5,
            PaymentPurpose = new PaymentPurpose
            {
                Id = 5,
                Code = "SUPPLIER_PAYMENT",
                Name = "Supplier payment",
                AliasId = 1,
                OperationTypeId = OperationTypeIdConst.OUT,
                RequiresCounterparty = true,
                Alias = new PostingAlias { Id = 1, Code = AliasConst.Supplier, Name = "Supplier" },
                OperationType = new OperationType { Id = OperationTypeIdConst.OUT, Code = "OUT", Name = "OUT" }
            },
            OperationTypeId = OperationTypeIdConst.OUT,
            PaymentTypeId = 2,
            CounterpartyId = 18,
            Counterparty = new CounterpartyCard { Id = 18, ShortName = "Vendor", OrganizationId = 8, CounterpartyTypeId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            DocNumber = "CASH-100",
            DocDate = new DateTime(2026, 7, 3),
            CurrencyId = 1,
            Amount = 700m,
            ExchangeRate = 1m,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        };

        var dispatcher = new CashOperationAccountingDispatcher();

        var service = new CashLifecycleService(
            new CashOperationUserContext(),
            new CashOperationTestQueryBuilder(),
            new CashOperationPostingLock(),
            new CashOperationPeriodValidator(),
            new CashOperationAuditLogService(),
            dispatcher,
            new CashOperationMoneyRegisterService(),
            new CashOperationCounterpartyRegisterService(),
            new CashOperationTestQueryRepository<CashOperation>([cashOperation]),
            new CashOperationCommandRepository<CashOperation>([cashOperation]),
            new CashOperationTestQueryRepository<PostingBatch>(postingBatches),
            new CashOperationCommandRepository<PostingBatch>(postingBatches, entity => entity.Id = entity.Id == 0 ? postingBatches.Count + 1 : entity.Id),
            new CashOperationTestQueryRepository<AccountingRegisterEntry>(accountingEntries),
            new CashOperationCommandRepository<AccountingRegisterEntry>(accountingEntries, entity => entity.Id = entity.Id == 0 ? accountingEntries.Count + 1 : entity.Id),
            new CashOperationTestQueryRepository<MoneyRegisterBalance>(moneyEntries),
            new CashOperationTestQueryRepository<CounterpartyRegisterBalance>(counterpartyEntries),
            NullLogger<CashLifecycleService>.Instance,
            new CashOperationUnitOfWork());

        return new CashLifecycleFixture
        {
            Service = service,
            CashOperation = cashOperation,
            Dispatcher = dispatcher,
            PostingBatches = postingBatches
        };
    }
}

file sealed class CashOperationUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class CashOperationUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class CashOperationDocNumberGenerator : IDocNumberGenerator
{
    public Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-000001");
}

file sealed class CashOperationLifecycleService : ICashLifecycleService
{
    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class CashOperationAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class CashOperationPostingLock : IDocumentPostingLock
{
    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.FromResult(true);
}

file sealed class CashOperationPeriodValidator : IAccountingPeriodValidator
{
    public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class CashOperationAccountingDispatcher : IAccountingDispatcher
{
    public int CallCount { get; private set; }

    public Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null)
    {
        CallCount++;
        return Task.FromResult(Result.Success(new List<AccountingRegisterEntry>()));
    }
}

file sealed class CashOperationMoneyRegisterService : ICashMoneyRegisterService
{
    public Task<Result<List<MoneyRegisterBalance>>> PostAsync(CashOperation cashOperation, long postingBatchId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<MoneyRegisterBalance> { new() { Id = 1, Amount = cashOperation.Amount } }));

    public Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(CashOperation cashOperation, long postingBatchId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<MoneyRegisterBalance> { new() { Id = 2, Amount = cashOperation.Amount } }));

    public Task<decimal> GetCashBoxBalanceAsync(int cashBoxId, DateTime asOfDate, CancellationToken ct = default) =>
        Task.FromResult(10_000m);
}

file sealed class CashOperationCounterpartyRegisterService : ICashCounterpartyRegisterService
{
    public Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(CashOperation cashOperation, long postingBatchId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<CounterpartyRegisterBalance> { new() { Id = 1, Amount = cashOperation.Amount } }));

    public Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(CashOperation cashOperation, long postingBatchId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<CounterpartyRegisterBalance> { new() { Id = 2, Amount = cashOperation.Amount } }));
}

file sealed class CashOperationPolicyResolver : IOrganizationAccountingPolicyResolver
{
    public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) =>
        Task.FromResult(AccountingPolicyIdConst.STANDARD_UZ);
}

file sealed class CashOperationCommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity>? _store;
    private readonly Action<TEntity>? _onCreate;

    public CashOperationCommandRepository(List<TEntity>? store = null, Action<TEntity>? onCreate = null)
    {
        _store = store;
        _onCreate = onCreate;
    }

    public List<TEntity> CreatedEntities { get; } = new();
    public List<TEntity> UpdatedEntities { get; } = new();

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
        if (_store != null)
        {
            var compiled = predicate.Compile();
            _store.RemoveAll(x => compiled(x));
        }

        return Task.CompletedTask;
    }

    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class CashOperationTestQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public CashOperationTestQueryRepository(IEnumerable<TEntity> data)
    {
        _data = data.ToList();
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

file sealed class CashOperationTestQueryBuilder : IQueryBuilder
{
    private static readonly CashOperationQueryBuilderResolver Resolver = new();

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

file sealed class CashOperationQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new CashOperationProjectionBuilder<TEntity, TResult>();
}

file sealed class CashOperationProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
