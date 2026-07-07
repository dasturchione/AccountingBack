using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.FaDepreciations;
using Application.Features.Register;
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

public class FaDepreciationPostingTests
{
    [Fact]
    public async Task Posting_ShouldCreateBalancedExpenseToAccumulatedDepreciationEntries()
    {
        var asset = FaDepreciationTestData.BuildAsset(methodCode: FaDepreciationMethodCodeConst.LINEAR);
        asset.DepreciationMethod = new FaDepreciationMethod
        {
            Id = 1,
            Code = FaDepreciationMethodCodeConst.LINEAR,
            Name = "Linear",
            StateId = StateIdConst.ACTIVE
        };

        var doc = new FaDepreciationRun
        {
            Id = 91,
            OrganizationId = 8,
            DocNumber = "FAD-2026-000001",
            PeriodMonth = new DateTime(2026, 7, 1),
            StatusId = DocumentStatusIdConst.POSTED,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today,
            UpdatedDate = DateTime.Today,
            Lines =
            [
                new FaDepreciationRunLine
                {
                    Id = 1,
                    DepreciationRunId = 91,
                    FaAssetId = asset.Id,
                    FaAsset = asset,
                    Amount = 150m
                }
            ]
        };

        var builder = new FaDepreciationRunContextBuilder(new FaDepreciationAccountingPolicyResolver());
        var contexts = await builder.BuildAsync(doc);

        var postingService = new PostingService(
            new FaDepreciationQueryBuilder(),
            new FaDepreciationQueryRepository<PostingRule>([BuildDepreciationRule()]),
            new FaDepreciationQueryRepository<AccountResolveRule>(
            [
                new AccountResolveRule { Id = 1, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAssetExpense, DimensionKey = "_none", DimensionValue = "_default", AccountId = 9430, Priority = 100 },
                new AccountResolveRule { Id = 2, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAssetDepreciation, DimensionKey = "_none", DimensionValue = "_default", AccountId = 290, Priority = 100 }
            ]),
            new FaDepreciationQueryRepository<ChartAccount>(
            [
                new ChartAccount { Id = 9430, Code = "9430", Name = "FA expense", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 290, Code = "0290", Name = "Accumulated depreciation", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
            ]));

        var entries = await postingService.BuildEntriesAsync(contexts);
        var validation = new AccountingPostingValidator().Validate(entries);

        Assert.True(validation.IsSuccess);
        Assert.Single(entries);
        Assert.Equal(150m, entries.Sum(x => x.Amount));
        Assert.Single(entries, x => x.DebitAccountId == 9430 && x.CreditAccountId == 290 && x.Amount == 150m);
        Assert.All(entries.SelectMany(x => x.RegisterEntrySubkontos), x => Assert.Equal(asset.Id, x.EntityId));
    }

    private static PostingRule BuildDepreciationRule()
    {
        var expense = new PostingAlias { Id = 38, Code = AliasConst.FixedAssetExpense, Name = "Expense" };
        var depreciation = new PostingAlias { Id = 37, Code = AliasConst.FixedAssetDepreciation, Name = "Accumulated depreciation" };

        return new PostingRule
        {
            Id = PostingRuleIdConst.FA_DEPRECIATION,
            Code = "FA_DEPRECIATION",
            Name = "Fixed asset depreciation",
            PostingRuleLines =
            [
                new PostingRuleLine
                {
                    Id = 28,
                    TemplateId = PostingRuleIdConst.FA_DEPRECIATION,
                    OrderNumber = 1,
                    DebitAliasId = expense.Id,
                    CreditAliasId = depreciation.Id,
                    AmountSource = AmountSourceConst.Base,
                    IsOptional = false,
                    DebitAlias = expense,
                    CreditAlias = depreciation
                }
            ]
        };
    }
}

public class FaDepreciationRunServiceTests
{
    [Fact]
    public async Task Run_ShouldCreatePostedRun_AndBalancedPostingEntries()
    {
        var fixture = FaDepreciationFixture.Create();

        var result = await fixture.Service.RunAsync("2026-07");

        Assert.True(result.IsSuccess);
        Assert.Equal(1, fixture.PostingLock.AcquireCount);
        Assert.Single(fixture.Runs);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.Runs.Single().StatusId);
        Assert.Equal(1, fixture.PostingBatches.Count(x => x.Status == PostingBatchStatusConst.POSTED));
        Assert.Single(fixture.AccountingEntries);
        Assert.Equal(9430, fixture.AccountingEntries.Single().DebitAccountId);
        Assert.Equal(290, fixture.AccountingEntries.Single().CreditAccountId);
        Assert.Equal(150m, fixture.AccountingEntries.Single().Amount);
    }

    [Fact]
    public async Task Run_ShouldRejectSecondRun_ForSamePeriod()
    {
        var fixture = FaDepreciationFixture.Create();
        var first = await fixture.Service.RunAsync("2026-07");
        Assert.True(first.IsSuccess);

        var second = await fixture.Service.RunAsync("2026-07");

        Assert.False(second.IsSuccess);
        Assert.Equal("FaDepreciation.AlreadyRun", second.Error.Code);
    }

    [Fact]
    public async Task Cancel_ShouldReversePosting_AndMarkRunCancelled()
    {
        var fixture = FaDepreciationFixture.Create();
        var run = await fixture.Service.RunAsync("2026-07");
        Assert.True(run.IsSuccess);

        var result = await fixture.Service.CancelAsync(run.Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, fixture.PostingLock.AcquireCount);
        Assert.Equal(DocumentStatusIdConst.CANCELLED, fixture.Runs.Single().StatusId);
        Assert.Equal(2, fixture.PostingBatches.Count);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSAL);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSED);
        Assert.Equal(2, fixture.AccountingEntries.Count);
        var original = fixture.AccountingEntries.Single(x => x.ReversalEntryId == null);
        var reversal = fixture.AccountingEntries.Single(x => x.ReversalEntryId == original.Id);
        Assert.Equal(original.DebitAccountId, reversal.CreditAccountId);
        Assert.Equal(original.CreditAccountId, reversal.DebitAccountId);
        Assert.Equal(original.Amount, reversal.Amount);
    }
}

file sealed class FaDepreciationFixture
{
    public required IFaDepreciationRunService Service { get; init; }
    public required FakeFaDepreciationPostingLock PostingLock { get; init; }
    public required List<FaDepreciationRun> Runs { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }
    public required List<AccountingRegisterEntry> AccountingEntries { get; init; }

    public static FaDepreciationFixture Create()
    {
        var asset = FaDepreciationTestData.BuildAsset(methodCode: FaDepreciationMethodCodeConst.LINEAR);
        asset.DepreciationMethod = new FaDepreciationMethod
        {
            Id = 1,
            Code = FaDepreciationMethodCodeConst.LINEAR,
            Name = "Linear",
            StateId = StateIdConst.ACTIVE
        };

        var runs = new List<FaDepreciationRun>();
        var postingBatches = new List<PostingBatch>();
        var accountingEntries = new List<AccountingRegisterEntry>();
        var postingLock = new FakeFaDepreciationPostingLock();
        var unitOfWork = new FakeFaDepreciationUnitOfWork();

        var service = new FaDepreciationRunService(
            new FakeFaDepreciationUserContext(),
            new FaDepreciationQueryBuilder(),
            new FakeFaDepreciationAuditLogService(),
            new FakeFaDepreciationDocNumberGenerator(),
            postingLock,
            new FakeFaDepreciationAccountingPeriodValidator(),
            new FakeFaDepreciationAccountingDispatcher(accountingEntries, postingBatches),
            new FaDepreciationQueryRepository<FaDepreciationRun>(runs),
            new FaDepreciationQueryRepository<FaAsset>([asset]),
            new FaDepreciationQueryRepository<FaDepreciationRunLine>(new List<FaDepreciationRunLine>()),
            new FakeFaDepreciationRunCommandRepository(runs),
            new FaDepreciationQueryRepository<PostingBatch>(postingBatches),
            new FakeFaDepreciationPostingBatchCommandRepository(postingBatches),
            new FaDepreciationQueryRepository<AccountingRegisterEntry>(accountingEntries),
            new FakeFaDepreciationAccountingEntryCommandRepository(accountingEntries),
            NullLogger<FaDepreciationRunService>.Instance,
            unitOfWork);

        return new FaDepreciationFixture
        {
            Service = service,
            PostingLock = postingLock,
            Runs = runs,
            PostingBatches = postingBatches,
            AccountingEntries = accountingEntries
        };
    }
}

file static class FaDepreciationTestData
{
    public static FaAsset BuildAsset(string methodCode) => new()
    {
        Id = 1000,
        OrganizationId = 8,
        StateId = StateIdConst.ACTIVE,
        InventoryNumber = "FA-1000",
        Name = "Lathe",
        FaGroupId = 1,
        DepreciationMethodId = 1,
        UsefulLifeMonths = 10,
        InitialCost = 1_500m,
        SalvageValue = 0m,
        DeprStartDate = new DateTime(2026, 7, 1),
        StatusId = FaAssetStatusIdConst.ACTIVE,
        CreatedDate = DateTime.Today,
        UpdatedDate = DateTime.Today,
        Status = new FaAssetStatus { Id = FaAssetStatusIdConst.ACTIVE, Code = "ACTIVE", Name = "Active", StateId = StateIdConst.ACTIVE },
        DepreciationMethod = new FaDepreciationMethod { Id = 1, Code = methodCode, Name = methodCode, StateId = StateIdConst.ACTIVE }
    };
}

file sealed class FakeFaDepreciationUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class FakeFaDepreciationUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeFaDepreciationPostingLock : IDocumentPostingLock
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

file sealed class FakeFaDepreciationAccountingPeriodValidator : IAccountingPeriodValidator
{
    public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

file sealed class FakeFaDepreciationDocNumberGenerator : IDocNumberGenerator
{
    public Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-{docDate:yyyy}-000001");
}

file sealed class FakeFaDepreciationAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class FakeFaDepreciationRunCommandRepository : IFaDepreciationRunCommandRepository
{
    private readonly List<FaDepreciationRun> _runs;

    public FakeFaDepreciationRunCommandRepository(List<FaDepreciationRun> runs)
    {
        _runs = runs;
    }

    public Task CreateAsync(FaDepreciationRun entity, CancellationToken ct = default)
    {
        entity.Id = _runs.Count + 1;
        long lineId = 1;
        foreach (var line in entity.Lines)
        {
            line.Id = lineId++;
            line.DepreciationRunId = entity.Id;
            line.DepreciationRun = entity;
        }

        _runs.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(FaDepreciationRun entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeFaDepreciationPostingBatchCommandRepository : ICommandRepository<PostingBatch>
{
    private readonly List<PostingBatch> _batches;

    public FakeFaDepreciationPostingBatchCommandRepository(List<PostingBatch> batches)
    {
        _batches = batches;
    }

    public Task CreateAsync(PostingBatch entity, CancellationToken ct = default)
    {
        entity.Id = _batches.Count + 1;
        _batches.Add(entity);
        return Task.CompletedTask;
    }

    public Task CreateAsync(IEnumerable<PostingBatch> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(PostingBatch entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(IEnumerable<PostingBatch> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(PostingBatch entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(IEnumerable<PostingBatch> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Expression<Func<PostingBatch, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
    public Task ReloadAsync(PostingBatch entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeFaDepreciationAccountingEntryCommandRepository : ICommandRepository<AccountingRegisterEntry>
{
    private readonly List<AccountingRegisterEntry> _entries;

    public FakeFaDepreciationAccountingEntryCommandRepository(List<AccountingRegisterEntry> entries)
    {
        _entries = entries;
    }

    public Task CreateAsync(AccountingRegisterEntry entity, CancellationToken ct = default)
    {
        entity.Id = _entries.Count + 1;
        long subkontoId = 1;
        foreach (var subkonto in entity.RegisterEntrySubkontos)
            subkonto.Id = subkontoId++;
        _entries.Add(entity);
        return Task.CompletedTask;
    }

    public Task CreateAsync(IEnumerable<AccountingRegisterEntry> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            entity.Id = _entries.Count + 1;
            long subkontoId = 1;
            foreach (var subkonto in entity.RegisterEntrySubkontos)
                subkonto.Id = subkontoId++;
            _entries.Add(entity);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(AccountingRegisterEntry entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(IEnumerable<AccountingRegisterEntry> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(AccountingRegisterEntry entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(IEnumerable<AccountingRegisterEntry> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Expression<Func<AccountingRegisterEntry, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
    public Task ReloadAsync(AccountingRegisterEntry entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeFaDepreciationAccountingDispatcher : IAccountingDispatcher
{
    private readonly List<AccountingRegisterEntry> _entries;
    private readonly List<PostingBatch> _batches;

    public FakeFaDepreciationAccountingDispatcher(List<AccountingRegisterEntry> entries, List<PostingBatch> batches)
    {
        _entries = entries;
        _batches = batches;
    }

    public Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null)
    {
        var run = (FaDepreciationRun)document;
        var batchId = postingBatchId ?? _batches.Single(x => x.DocumentId == run.Id && x.Status == PostingBatchStatusConst.POSTED).Id;

        var created = run.Lines.Select(line => new AccountingRegisterEntry
        {
            Id = _entries.Count + 1,
            OrganizationId = run.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.FADEPRECIATION,
            DocumentId = run.Id,
            DebitAccountId = 9430,
            CreditAccountId = 290,
            CurrencyId = CurrencyIdConst.UZS,
            Amount = line.Amount,
            DocDate = run.PeriodMonth,
            CreatedDate = DateTime.Now,
            Content = "Fixed asset depreciation",
            JournalNumber = run.DocNumber,
            PostingBatchId = batchId,
            SourceLineId = line.Id,
            RegisterEntrySubkontos =
            [
                new RegisterEntrySubkonto
                {
                    Id = 1,
                    Side = SubkontoSideConst.DEBIT,
                    SubkontoTypeId = SubkontoTypeIdConst.FIXED_ASSET,
                    SortOrder = 1,
                    EntityId = line.FaAssetId,
                    DisplayValue = line.FaAsset?.Name,
                    CreatedDate = DateTime.Now
                },
                new RegisterEntrySubkonto
                {
                    Id = 2,
                    Side = SubkontoSideConst.CREDIT,
                    SubkontoTypeId = SubkontoTypeIdConst.FIXED_ASSET,
                    SortOrder = 1,
                    EntityId = line.FaAssetId,
                    DisplayValue = line.FaAsset?.Name,
                    CreatedDate = DateTime.Now
                }
            ]
        }).ToList();

        _entries.AddRange(created);
        return Task.FromResult(Result.Success(created));
    }
}

file sealed class FaDepreciationAccountingPolicyResolver : IOrganizationAccountingPolicyResolver
{
    public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) =>
        Task.FromResult(AccountingPolicyIdConst.STANDARD_UZ);
}

file sealed class FaDepreciationQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public FaDepreciationQueryRepository(IEnumerable<TEntity> data)
    {
        _data = data as List<TEntity> ?? data.ToList();
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
        var filtered = _data.AsQueryable().Where(specification.Criteria);
        return Task.FromResult(new PagedList<TEntity>(filtered.Skip(specification.Skip).Take(take).ToList(), filtered.Count()));
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

file sealed class FaDepreciationQueryBuilder : IQueryBuilder
{
    private static readonly FaDepreciationQueryBuilderResolver Resolver = new();

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

file sealed class FaDepreciationQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new FaDepreciationProjectionBuilder<TEntity, TResult>();
}

file sealed class FaDepreciationProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
