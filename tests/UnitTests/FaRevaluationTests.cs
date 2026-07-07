using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.FaAssets;
using Application.Features.FaRevaluations;
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

public class FaRevaluationPostingTests
{
    [Fact]
    public async Task Posting_ShouldCreateIncreaseEntry_ForPositiveDifference()
    {
        var asset = FaRevaluationTestData.BuildAsset();
        var doc = new FaRevaluationDoc
        {
            Id = 1,
            OrganizationId = 8,
            DocNumber = "FARV-2026-000001",
            RevaluationDate = new DateTime(2026, 8, 1),
            StatusId = DocumentStatusIdConst.POSTED,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today,
            UpdatedDate = DateTime.Today,
            Lines =
            [
                new FaRevaluationDocLine
                {
                    Id = 1,
                    RevaluationDocId = 1,
                    FaAssetId = asset.Id,
                    FaAsset = asset,
                    OldValue = 400m,
                    NewValue = 550m,
                    RevaluationAmount = 150m
                }
            ]
        };

        var builder = new FaRevaluationContextBuilder(new FaRevaluationAccountingPolicyResolver());
        var contexts = await builder.BuildAsync(doc);

        var fixedAsset = new PostingAlias { Id = 35, Code = AliasConst.FixedAsset, Name = "Fixed asset" };
        var surplus = new PostingAlias { Id = 41, Code = AliasConst.FixedAssetRevaluationSurplus, Name = "Revaluation surplus" };
        var loss = new PostingAlias { Id = 42, Code = AliasConst.FixedAssetRevaluationLoss, Name = "Revaluation loss" };

        var rules = new List<PostingRule>
        {
            new()
            {
                Id = PostingRuleIdConst.FA_REVALUATION_INCREASE,
                Code = "FA_REVALUATION_INCREASE",
                Name = "Revaluation increase",
                PostingRuleLines =
                [
                    new PostingRuleLine
                    {
                        Id = 32,
                        TemplateId = PostingRuleIdConst.FA_REVALUATION_INCREASE,
                        OrderNumber = 1,
                        DebitAliasId = fixedAsset.Id,
                        CreditAliasId = surplus.Id,
                        AmountSource = "Increase",
                        IsOptional = false,
                        DebitAlias = fixedAsset,
                        CreditAlias = surplus
                    }
                ]
            },
            new()
            {
                Id = PostingRuleIdConst.FA_REVALUATION_DECREASE,
                Code = "FA_REVALUATION_DECREASE",
                Name = "Revaluation decrease",
                PostingRuleLines =
                [
                    new PostingRuleLine
                    {
                        Id = 33,
                        TemplateId = PostingRuleIdConst.FA_REVALUATION_DECREASE,
                        OrderNumber = 1,
                        DebitAliasId = loss.Id,
                        CreditAliasId = fixedAsset.Id,
                        AmountSource = "Decrease",
                        IsOptional = false,
                        DebitAlias = loss,
                        CreditAlias = fixedAsset
                    }
                ]
            }
        };

        var postingService = new PostingService(
            new FaRevaluationQueryBuilder(),
            new FaRevaluationQueryRepository<PostingRule>(rules),
            new FaRevaluationQueryRepository<AccountResolveRule>(
            [
                new AccountResolveRule { Id = 1, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAsset, DimensionKey = "_none", DimensionValue = "_default", AccountId = 190, Priority = 100 },
                new AccountResolveRule { Id = 2, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAssetRevaluationSurplus, DimensionKey = "_none", DimensionValue = "_default", AccountId = 90301, Priority = 100 },
                new AccountResolveRule { Id = 3, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAssetRevaluationLoss, DimensionKey = "_none", DimensionValue = "_default", AccountId = 9430, Priority = 100 }
            ]),
            new FaRevaluationQueryRepository<ChartAccount>(
            [
                new ChartAccount { Id = 190, Code = "0190", Name = "Fixed asset", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 90301, Code = "9030.1", Name = "Revaluation surplus", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 9430, Code = "9430", Name = "Revaluation loss", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
            ]));

        var entries = await postingService.BuildEntriesAsync(contexts);
        var validation = new AccountingPostingValidator().Validate(entries);

        Assert.True(validation.IsSuccess);
        Assert.Single(entries);
        Assert.Single(entries, x => x.DebitAccountId == 190 && x.CreditAccountId == 90301 && x.Amount == 150m);
    }
}

public class FaRevaluationLifecycleTests
{
    [Fact]
    public async Task Confirm_ShouldIncreaseAssetInitialCost_WhenDifferencePositive()
    {
        var fixture = FaRevaluationFixture.Create(newValue: 550m);

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.Doc.StatusId);
        Assert.Equal(400m, fixture.Doc.Lines.Single().OldValue);
        Assert.Equal(150m, fixture.Doc.Lines.Single().RevaluationAmount);
        Assert.Equal(750m, fixture.Asset.InitialCost);
        Assert.Single(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.POSTED);
    }

    [Fact]
    public async Task Confirm_ShouldDecreaseAssetInitialCost_WhenDifferenceNegative()
    {
        var fixture = FaRevaluationFixture.Create(newValue: 300m);

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(-100m, fixture.Doc.Lines.Single().RevaluationAmount);
        Assert.Equal(500m, fixture.Asset.InitialCost);
    }

    [Fact]
    public async Task Cancel_ShouldRestoreInitialCost_AndReversePosting()
    {
        var fixture = FaRevaluationFixture.Create(newValue: 550m);
        var confirm = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);
        Assert.True(confirm.IsSuccess);

        var result = await fixture.LifecycleService.CancelAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatusIdConst.CANCELLED, fixture.Doc.StatusId);
        Assert.Equal(600m, fixture.Asset.InitialCost);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSAL);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSED);
    }

    [Fact]
    public async Task Confirm_ShouldRejectDisposedAsset()
    {
        var fixture = FaRevaluationFixture.Create(newValue: 550m);
        fixture.Asset.StatusId = FaAssetStatusIdConst.DISPOSED;
        fixture.Asset.Status = new FaAssetStatus
        {
            Id = FaAssetStatusIdConst.DISPOSED,
            Code = "DISPOSED",
            Name = "Disposed",
            StateId = StateIdConst.ACTIVE
        };

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("FaRevaluation.AssetDisposed", result.Error.Code);
        Assert.Equal(DocumentStatusIdConst.DRAFT, fixture.Doc.StatusId);
    }
}

file sealed class FaRevaluationFixture
{
    public required FaRevaluationLifecycleService LifecycleService { get; init; }
    public required FaRevaluationDoc Doc { get; init; }
    public required FaAsset Asset { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }

    public static FaRevaluationFixture Create(decimal newValue)
    {
        var asset = FaRevaluationTestData.BuildAsset();
        var doc = new FaRevaluationDoc
        {
            Id = 1,
            OrganizationId = 8,
            StateId = StateIdConst.ACTIVE,
            DocNumber = "FARV-2026-000001",
            RevaluationDate = new DateTime(2026, 8, 1),
            StatusId = DocumentStatusIdConst.DRAFT,
            CreatedDate = DateTime.Today,
            UpdatedDate = DateTime.Today,
            Lines =
            [
                new FaRevaluationDocLine
                {
                    Id = 1,
                    RevaluationDocId = 1,
                    FaAssetId = asset.Id,
                    FaAsset = asset,
                    NewValue = newValue
                }
            ]
        };
        doc.Lines.Single().RevaluationDoc = doc;

        var postingBatches = new List<PostingBatch>();
        var accountingEntries = new List<AccountingRegisterEntry>();
        var depreciationLines = new List<FaDepreciationRunLine>
        {
            new()
            {
                Id = 1,
                DepreciationRunId = 1,
                FaAssetId = asset.Id,
                Amount = 200m,
                DepreciationRun = new FaDepreciationRun
                {
                    Id = 1,
                    OrganizationId = 8,
                    StateId = StateIdConst.ACTIVE,
                    DocNumber = "FAD-2026-000001",
                    PeriodMonth = new DateTime(2026, 8, 1),
                    StatusId = DocumentStatusIdConst.POSTED,
                    CreatedDate = DateTime.Today,
                    UpdatedDate = DateTime.Today
                }
            }
        };

        var service = new FaRevaluationLifecycleService(
            new FakeFaRevaluationUserContext(),
            new FakeFaRevaluationUnitOfWork(),
            new FaRevaluationQueryBuilder(),
            new FakeFaRevaluationPostingLock(),
            new FakeFaRevaluationAccountingPeriodValidator(),
            new FakeFaRevaluationAuditLogService(),
            new FakeFaRevaluationAccountingDispatcher(accountingEntries, postingBatches),
            new FaRevaluationQueryRepository<FaRevaluationDoc>(new List<FaRevaluationDoc> { doc }),
            new FakeFaRevaluationCommandRepository(new List<FaRevaluationDoc> { doc }),
            new FakeFaRevaluationAssetCommandRepository(new List<FaAsset> { asset }),
            new FaRevaluationQueryRepository<FaDepreciationRunLine>(depreciationLines),
            new FaRevaluationQueryRepository<PostingBatch>(postingBatches),
            new FakeFaRevaluationPostingBatchCommandRepository(postingBatches),
            new FaRevaluationQueryRepository<AccountingRegisterEntry>(accountingEntries),
            new FakeFaRevaluationAccountingEntryCommandRepository(accountingEntries),
            NullLogger<FaRevaluationLifecycleService>.Instance);

        return new FaRevaluationFixture
        {
            LifecycleService = service,
            Doc = doc,
            Asset = asset,
            PostingBatches = postingBatches
        };
    }
}

file static class FaRevaluationTestData
{
    public static FaAsset BuildAsset() => new()
    {
        Id = 1000,
        OrganizationId = 8,
        StateId = StateIdConst.ACTIVE,
        InventoryNumber = "FA-1000",
        Name = "Machine",
        FaGroupId = 1,
        DepreciationMethodId = 1,
        UsefulLifeMonths = 12,
        InitialCost = 600m,
        SalvageValue = 0,
        StatusId = FaAssetStatusIdConst.ACTIVE,
        CreatedDate = DateTime.Today,
        UpdatedDate = DateTime.Today,
        Status = new FaAssetStatus { Id = FaAssetStatusIdConst.ACTIVE, Code = "ACTIVE", Name = "Active", StateId = StateIdConst.ACTIVE }
    };
}

file sealed class FakeFaRevaluationUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class FakeFaRevaluationUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeFaRevaluationPostingLock : IDocumentPostingLock
{
    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.FromResult(true);
}

file sealed class FakeFaRevaluationAccountingPeriodValidator : IAccountingPeriodValidator
{
    public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class FakeFaRevaluationAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class FakeFaRevaluationCommandRepository : IFaRevaluationCommandRepository
{
    private readonly List<FaRevaluationDoc> _docs;

    public FakeFaRevaluationCommandRepository(List<FaRevaluationDoc> docs)
    {
        _docs = docs;
    }

    public Task CreateAsync(FaRevaluationDoc entity, CancellationToken ct = default)
    {
        _docs.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(FaRevaluationDoc entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteLinesAsync(IEnumerable<FaRevaluationDocLine> entities, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeFaRevaluationAssetCommandRepository : IFaAssetCommandRepository
{
    private readonly List<FaAsset> _assets;

    public FakeFaRevaluationAssetCommandRepository(List<FaAsset> assets)
    {
        _assets = assets;
    }

    public Task CreateAsync(FaAsset entity, CancellationToken ct = default)
    {
        _assets.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(FaAsset entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeFaRevaluationPostingBatchCommandRepository : ICommandRepository<PostingBatch>
{
    private readonly List<PostingBatch> _batches;

    public FakeFaRevaluationPostingBatchCommandRepository(List<PostingBatch> batches)
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

file sealed class FakeFaRevaluationAccountingEntryCommandRepository : ICommandRepository<AccountingRegisterEntry>
{
    private readonly List<AccountingRegisterEntry> _entries;

    public FakeFaRevaluationAccountingEntryCommandRepository(List<AccountingRegisterEntry> entries)
    {
        _entries = entries;
    }

    public Task CreateAsync(AccountingRegisterEntry entity, CancellationToken ct = default)
    {
        entity.Id = _entries.Count + 1;
        _entries.Add(entity);
        return Task.CompletedTask;
    }

    public Task CreateAsync(IEnumerable<AccountingRegisterEntry> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            entity.Id = _entries.Count + 1;
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

file sealed class FakeFaRevaluationAccountingDispatcher : IAccountingDispatcher
{
    private readonly List<AccountingRegisterEntry> _entries;
    private readonly List<PostingBatch> _batches;

    public FakeFaRevaluationAccountingDispatcher(List<AccountingRegisterEntry> entries, List<PostingBatch> batches)
    {
        _entries = entries;
        _batches = batches;
    }

    public Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null)
    {
        var doc = (FaRevaluationDoc)document;
        var batchId = postingBatchId ?? _batches.Single().Id;
        var line = doc.Lines.Single();
        var amount = Math.Abs(line.RevaluationAmount);
        if (amount == 0m)
            return Task.FromResult(Result.Success(new List<AccountingRegisterEntry>()));

        var entry = new AccountingRegisterEntry
        {
            Id = _entries.Count + 1,
            OrganizationId = doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.FAREVALUATION,
            DocumentId = doc.Id,
            DebitAccountId = line.RevaluationAmount >= 0m ? 190 : 9430,
            CreditAccountId = line.RevaluationAmount >= 0m ? 90301 : 190,
            CurrencyId = CurrencyIdConst.UZS,
            Amount = amount,
            DocDate = doc.RevaluationDate,
            CreatedDate = DateTime.Now,
            PostingBatchId = batchId
        };
        _entries.Add(entry);
        return Task.FromResult(Result.Success(new List<AccountingRegisterEntry> { entry }));
    }
}

file sealed class FaRevaluationAccountingPolicyResolver : IOrganizationAccountingPolicyResolver
{
    public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) => Task.FromResult(AccountingPolicyIdConst.STANDARD_UZ);
}

file sealed class FaRevaluationQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public FaRevaluationQueryRepository(IEnumerable<TEntity> data)
    {
        _data = data as List<TEntity> ?? data.ToList();
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.FromResult(_data.AsQueryable().Any(predicate));
    public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) => Task.FromResult(_data.AsQueryable().Where(specification.Criteria).FirstOrDefault());
    public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) => Task.FromResult(_data.AsQueryable().Where(specification.Criteria).Select(specification.Selector).FirstOrDefault());
    public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) => Task.FromResult(_data.AsQueryable().Where(specification.Criteria).ToList());
    public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) => Task.FromResult(_data.AsQueryable().Where(specification.Criteria).Select(specification.Selector).ToList());

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

file sealed class FaRevaluationQueryBuilder : IQueryBuilder
{
    private static readonly FaRevaluationQueryBuilderResolver Resolver = new();

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

file sealed class FaRevaluationQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new FaRevaluationProjectionBuilder<TEntity, TResult>();
}

file sealed class FaRevaluationProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
