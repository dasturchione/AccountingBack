using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.BankOperations;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.MoneyRegisterBalances;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.Register.PostingEngines;
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

public class BankOperationModelTests
{
    [Fact]
    public async Task Create_ShouldPersistPaymentPurposeOnBankOperationHeader()
    {
        var fixture = BankOperationServiceFixture.Create();

        var result = await fixture.Service.CreateAsync(fixture.BuildCreateDto());

        Assert.True(result.IsSuccess);
        var created = fixture.Command.CreatedEntities.Single();
        Assert.Equal(5, created.PaymentPurposeId);
        Assert.Single(created.BankOperationLines);
        var line = created.BankOperationLines.Single();
        Assert.Equal(5, line.PaymentPurposeId);
        Assert.Equal(1250m, line.Amount);
    }

    [Fact]
    public async Task ContextBuilder_ShouldUseHeaderPaymentPurpose_WhenLinesAreEmpty()
    {
        var operation = new BankOperation
        {
            Id = 100,
            OrganizationId = 8,
            BankAccountId = 4,
            PaymentPurposeId = 5,
            OperationTypeId = OperationTypeIdConst.OUT,
            PaymentTypeId = 2,
            CounterpartyId = 18,
            ContractId = 9,
            DocNumber = "BNK-100",
            DocDate = new DateTime(2026, 7, 3),
            CurrencyId = 1,
            Amount = 1250m,
            ExchangeRate = 1m,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        };

        var builder = new BankOperationContextBuilder(
            new BankOperationTestQueryBuilder(),
            new BankOperationTestQueryRepository<Contract>(
            [
                new Contract { Id = 9, ContractNumber = "C-9", ContractDate = DateTime.Today, CounterpartyId = 18, ContractTypeId = 1, StartDate = DateTime.Today, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
            ]),
            new BankOperationTestQueryRepository<BankAccount>(
            [
                new BankAccount { Id = 4, AccountNumber = "2020", Name = "Main", OrganizationId = 8, CurrencyId = 1, BankId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
            ]),
            new BankOperationTestQueryRepository<PaymentType>(
            [
                new PaymentType { Id = 2, Code = "WIRE", Name = "Wire" }
            ]),
            new BankOperationTestQueryRepository<CounterpartyCard>(
            [
                new CounterpartyCard { Id = 18, ShortName = "Vendor", OrganizationId = 8, CounterpartyTypeId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
            ]),
            new BankOperationTestQueryRepository<PaymentPurpose>(
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
            new BankOperationPolicyResolver());

        var contexts = await builder.BuildAsync(operation);

        Assert.Single(contexts);
        var context = contexts.Single();
        Assert.Equal(PostingRuleIdConst.CREDIT_OPERATION, context.RuleId);
        Assert.Equal(AliasConst.Supplier, context.RequiredDebitAlias);
        Assert.Equal(AliasConst.PaymentAccount, context.RequiredCreditAlias);
        Assert.Equal(1250m, context.Amounts[AmountSourceConst.Total]);
    }

    [Fact]
    public async Task Confirm_ShouldValidateHeaderPaymentPurpose_WithoutBankOperationLines()
    {
        var fixture = BankLifecycleFixture.Create();

        var result = await fixture.Service.ConfirmAsync(fixture.BankOperation.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.BankOperation.StatusId);
        Assert.Equal(1, fixture.Dispatcher.CallCount);
        Assert.Single(fixture.PostingBatches);
    }

    [Fact]
    public async Task Confirm_ShouldAllowCashCollectionReceiptWithoutCounterparty()
    {
        var fixture = BankLifecycleFixture.Create();
        fixture.BankOperation.PaymentPurposeId = 6;
        fixture.BankOperation.PaymentPurpose = new PaymentPurpose
        {
            Id = 6,
            Code = "CASH_COLLECTION_RECEIVED",
            Name = "Cash collection from transit",
            AliasId = 2,
            OperationTypeId = OperationTypeIdConst.IN,
            RequiresCounterparty = false,
            Alias = new PostingAlias { Id = 2, Code = AliasConst.CashInTransit, Name = "Cash in transit" },
            OperationType = new OperationType { Id = OperationTypeIdConst.IN, Code = "IN", Name = "IN" }
        };
        fixture.BankOperation.OperationTypeId = OperationTypeIdConst.IN;
        fixture.BankOperation.CounterpartyId = null;
        fixture.BankOperation.Counterparty = null;
        fixture.BankOperation.CounterpartyBankAccountId = null;
        fixture.BankOperation.CounterpartyBankAccount = null;
        fixture.BankOperation.ContractId = null;
        fixture.BankOperation.Contract = null;

        var result = await fixture.Service.ConfirmAsync(fixture.BankOperation.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.BankOperation.StatusId);
    }
}

file sealed class BankOperationServiceFixture
{
    public required BankOperationService Service { get; init; }
    public required BankOperationCommandRepository<BankOperation> Command { get; init; }

    public static BankOperationServiceFixture Create()
    {
        var docs = new List<BankOperation>();
        var command = new BankOperationCommandRepository<BankOperation>(docs, entity => entity.Id = entity.Id == 0 ? 100 : entity.Id);

        var service = new BankOperationService(
            new BankOperationUserContext(),
            new BankOperationTestQueryBuilder(),
            new BankOperationAuditLogService(),
            new BankOperationLifecycleService(),
            new BankOperationDocNumberGenerator(),
            new BankOperationTestQueryRepository<BankOperation>(docs),
            command,
            NullLogger<BankOperationService>.Instance,
            new BankOperationUnitOfWork());

        return new BankOperationServiceFixture
        {
            Service = service,
            Command = command
        };
    }

    public BankOperationCreateDto BuildCreateDto() => new()
    {
        BankAccountId = 4,
        PaymentPurposeId = 5,
        OperationTypeId = OperationTypeIdConst.OUT,
        PaymentTypeId = 2,
        CounterpartyId = 18,
        CounterpartyBankAccountId = 11,
        ContractId = 9,
        DocDate = new DateTime(2026, 7, 3),
        CurrencyId = 1,
        Amount = 1250m,
        ExchangeRate = 1m,
        Comment = "payment"
    };
}

file sealed class BankLifecycleFixture
{
    public required BankLifecycleService Service { get; init; }
    public required BankOperation BankOperation { get; init; }
    public required BankOperationAccountingDispatcher Dispatcher { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }

    public static BankLifecycleFixture Create()
    {
        var postingBatches = new List<PostingBatch>();
        var accountingEntries = new List<AccountingRegisterEntry>();
        var moneyEntries = new List<MoneyRegisterBalance>();
        var counterpartyEntries = new List<CounterpartyRegisterBalance>();

        var bankOperation = new BankOperation
        {
            Id = 100,
            OrganizationId = 8,
            BankAccountId = 4,
            BankAccount = new BankAccount { Id = 4, AccountNumber = "2020", Name = "Main", OrganizationId = 8, CurrencyId = 1, BankId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
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
            CounterpartyBankAccountId = 11,
            CounterpartyBankAccount = new CounterpartyBankAccount { Id = 11, OrganizationId = 8, CounterpartyId = 18, AccountNumber = "998", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            ContractId = 9,
            Contract = new Contract { Id = 9, ContractNumber = "C-9", ContractDate = DateTime.Today, CounterpartyId = 18, ContractTypeId = 1, StartDate = DateTime.Today, OrganizationId = 8, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
            DocNumber = "BNK-100",
            DocDate = new DateTime(2026, 7, 3),
            CurrencyId = 1,
            Amount = 1250m,
            ExchangeRate = 1m,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today,
            BankOperationLines = []
        };

        var dispatcher = new BankOperationAccountingDispatcher();

        var service = new BankLifecycleService(
            new BankOperationUserContext(),
            new BankOperationPermissionChecker(),
            new BankOperationTestQueryBuilder(),
            new BankOperationPostingLock(),
            new BankOperationPeriodValidator(),
            new BankOperationAuditLogService(),
            dispatcher,
            new BankOperationMoneyRegisterService(),
            new BankOperationCounterpartyRegisterService(),
            new BankOperationTestQueryRepository<BankOperation>([bankOperation]),
            new BankOperationCommandRepository<BankOperation>([bankOperation]),
            new BankOperationTestQueryRepository<PostingBatch>(postingBatches),
            new BankOperationCommandRepository<PostingBatch>(postingBatches, entity => entity.Id = entity.Id == 0 ? postingBatches.Count + 1 : entity.Id),
            new BankOperationTestQueryRepository<AccountingRegisterEntry>(accountingEntries),
            new BankOperationCommandRepository<AccountingRegisterEntry>(accountingEntries, entity => entity.Id = entity.Id == 0 ? accountingEntries.Count + 1 : entity.Id),
            new BankOperationTestQueryRepository<MoneyRegisterBalance>(moneyEntries),
            new BankOperationTestQueryRepository<CounterpartyRegisterBalance>(counterpartyEntries),
            NullLogger<BankLifecycleService>.Instance,
            new BankOperationUnitOfWork());

        return new BankLifecycleFixture
        {
            Service = service,
            BankOperation = bankOperation,
            Dispatcher = dispatcher,
            PostingBatches = postingBatches
        };
    }
}

file sealed class BankOperationUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class BankOperationUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class BankOperationDocNumberGenerator : IDocNumberGenerator
{
    public Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-000001");
}

file sealed class BankOperationLifecycleService : IBankLifecycleService
{
    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class BankOperationAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class BankOperationPermissionChecker : IPermissionChecker
{
    public Task<bool> HasPermissionAsync(int roleId, string permissionCode, CancellationToken ct = default) => Task.FromResult(true);
    public Task<bool> HasAnyPermissionAsync(int roleId, IEnumerable<string> permissionCodes, CancellationToken ct = default) => Task.FromResult(true);
}

file sealed class BankOperationPostingLock : IDocumentPostingLock
{
    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.FromResult(true);
}

file sealed class BankOperationPeriodValidator : IAccountingPeriodValidator
{
    public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class BankOperationAccountingDispatcher : IAccountingDispatcher
{
    public int CallCount { get; private set; }

    public Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null)
    {
        CallCount++;
        return Task.FromResult(Result.Success(new List<AccountingRegisterEntry>()));
    }
}

file sealed class BankOperationMoneyRegisterService : IBankMoneyRegisterService
{
    public Task<Result<List<MoneyRegisterBalance>>> PostAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<MoneyRegisterBalance> { new() { Id = 1, Amount = bankOperation.Amount } }));

    public Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<MoneyRegisterBalance> { new() { Id = 2, Amount = bankOperation.Amount } }));

    public Task<decimal> GetBankAccountBalanceAsync(int bankAccountId, DateTime asOfDate, CancellationToken ct = default) =>
        Task.FromResult(10_000m);
}

file sealed class BankOperationCounterpartyRegisterService : IBankCounterpartyRegisterService
{
    public Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<CounterpartyRegisterBalance> { new() { Id = 1, Amount = bankOperation.Amount } }));

    public Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<CounterpartyRegisterBalance> { new() { Id = 2, Amount = bankOperation.Amount } }));
}

file sealed class BankOperationPolicyResolver : IOrganizationAccountingPolicyResolver
{
    public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) =>
        Task.FromResult(AccountingPolicyIdConst.STANDARD_UZ);
}

file sealed class BankOperationCommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity>? _store;
    private readonly Action<TEntity>? _onCreate;

    public BankOperationCommandRepository(List<TEntity>? store = null, Action<TEntity>? onCreate = null)
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

file sealed class BankOperationTestQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public BankOperationTestQueryRepository(IEnumerable<TEntity> data)
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

file sealed class BankOperationTestQueryBuilder : IQueryBuilder
{
    private static readonly BankOperationQueryBuilderResolver Resolver = new();

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

file sealed class BankOperationQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new BankOperationProjectionBuilder<TEntity, TResult>();
}

file sealed class BankOperationProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
