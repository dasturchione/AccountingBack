using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.FaAssets;
using Application.Features.FaDisposals;
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

public class FaDisposalPostingTests
{
    [Fact]
    public async Task Posting_ShouldBeBalanced_ForSaleDisposal()
    {
        var asset = FaDisposalTestData.BuildAsset();
        var doc = new FaDisposalDoc
        {
            Id = 1,
            OrganizationId = 8,
            DocNumber = "FADS-2026-000001",
            DisposalDate = new DateTime(2026, 8, 1),
            DisposalType = "SALE",
            StatusId = DocumentStatusIdConst.POSTED,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today,
            UpdatedDate = DateTime.Today,
            Lines =
            [
                new FaDisposalDocLine
                {
                    Id = 1,
                    DisposalDocId = 1,
                    FaAssetId = asset.Id,
                    FaAsset = asset,
                    BookValue = 400m,
                    SaleAmount = 500m,
                    GainLoss = 100m
                }
            ]
        };

        var builder = new FaDisposalContextBuilder(new FaDisposalAccountingPolicyResolver());
        var contexts = await builder.BuildAsync(doc);

        var fixedAsset = new PostingAlias { Id = 35, Code = AliasConst.FixedAsset, Name = "Fixed asset" };
        var accumulated = new PostingAlias { Id = 37, Code = AliasConst.FixedAssetDepreciation, Name = "Accumulated depreciation" };
        var proceeds = new PostingAlias { Id = 40, Code = AliasConst.FixedAssetDisposalGain, Name = "Disposal proceeds" };
        var payment = new PostingAlias { Id = 33, Code = AliasConst.PaymentAccount, Name = "Payment account" };
        var loss = new PostingAlias { Id = 39, Code = AliasConst.FixedAssetDisposalLoss, Name = "Loss" };

        var rule = new PostingRule
        {
            Id = PostingRuleIdConst.FA_DISPOSAL,
            Code = "FA_DISPOSAL",
            Name = "Fixed asset disposal",
            PostingRuleLines =
            [
                new PostingRuleLine
                {
                    Id = 29, TemplateId = PostingRuleIdConst.FA_DISPOSAL, OrderNumber = 1,
                    DebitAliasId = accumulated.Id, CreditAliasId = fixedAsset.Id,
                    AmountSource = "Accumulated", IsOptional = false,
                    DebitAlias = accumulated, CreditAlias = fixedAsset
                },
                new PostingRuleLine
                {
                    Id = 30, TemplateId = PostingRuleIdConst.FA_DISPOSAL, OrderNumber = 2,
                    DebitAliasId = payment.Id, CreditAliasId = proceeds.Id,
                    AmountSource = "Sale", IsOptional = true,
                    DebitAlias = payment, CreditAlias = proceeds
                },
                new PostingRuleLine
                {
                    Id = 31, TemplateId = PostingRuleIdConst.FA_DISPOSAL, OrderNumber = 3,
                    DebitAliasId = loss.Id, CreditAliasId = fixedAsset.Id,
                    AmountSource = "Loss", IsOptional = true,
                    DebitAlias = loss, CreditAlias = fixedAsset
                }
            ]
        };

        var postingService = new PostingService(
            new FaDisposalQueryBuilder(),
            new FaDisposalQueryRepository<PostingRule>([rule]),
            new FaDisposalQueryRepository<AccountResolveRule>(
            [
                new AccountResolveRule { Id = 1, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAsset, DimensionKey = "_none", DimensionValue = "_default", AccountId = 190, Priority = 100 },
                new AccountResolveRule { Id = 2, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAssetDepreciation, DimensionKey = "_none", DimensionValue = "_default", AccountId = 290, Priority = 100 },
                new AccountResolveRule { Id = 3, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.PaymentAccount, DimensionKey = "paymentMethod", DimensionValue = "_default", AccountId = 5110, Priority = 100 },
                new AccountResolveRule { Id = 4, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAssetDisposalGain, DimensionKey = "_none", DimensionValue = "_default", AccountId = 90301, Priority = 100 },
                new AccountResolveRule { Id = 5, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAssetDisposalLoss, DimensionKey = "_none", DimensionValue = "_default", AccountId = 9430, Priority = 100 }
            ]),
            new FaDisposalQueryRepository<ChartAccount>(
            [
                new ChartAccount { Id = 190, Code = "0190", Name = "Fixed asset", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 290, Code = "0290", Name = "Accumulated depreciation", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 5110, Code = "5110", Name = "Payment account", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 90301, Code = "9030.1", Name = "Disposal proceeds", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 9430, Code = "9430", Name = "Disposal loss", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
            ]));

        var entries = await postingService.BuildEntriesAsync(contexts);
        var validation = new AccountingPostingValidator().Validate(entries);

        Assert.True(validation.IsSuccess);
        Assert.Equal(2, entries.Count);
        Assert.Equal(entries.Sum(x => x.Amount), entries.Where(x => x.DebitAccountId.HasValue).Sum(x => x.Amount));
        Assert.Single(entries, x => x.DebitAccountId == 290 && x.CreditAccountId == 190 && x.Amount == 600m);
        Assert.Single(entries, x => x.DebitAccountId == 5110 && x.CreditAccountId == 90301 && x.Amount == 500m);
    }
}

public class FaDisposalLifecycleTests
{
    [Fact]
    public async Task Confirm_ShouldDisposeAsset_AndCreateBalancedEntries()
    {
        var fixture = FaDisposalFixture.Create();

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(FaAssetStatusIdConst.DISPOSED, fixture.Asset.StatusId);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.Doc.StatusId);
        Assert.Equal(400m, fixture.Doc.Lines.Single().BookValue);
        Assert.Equal(100m, fixture.Doc.Lines.Single().GainLoss);
        Assert.Single(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.POSTED);
        Assert.Equal(2, fixture.AccountingEntries.Count);
    }

    [Fact]
    public async Task Cancel_ShouldRestoreAsset_AndWriteReversal()
    {
        var fixture = FaDisposalFixture.Create();
        var confirm = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);
        Assert.True(confirm.IsSuccess);

        var result = await fixture.LifecycleService.CancelAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(FaAssetStatusIdConst.ACTIVE, fixture.Asset.StatusId);
        Assert.Equal(DocumentStatusIdConst.CANCELLED, fixture.Doc.StatusId);
        Assert.Equal(2, fixture.PostingBatches.Count);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSED);
        Assert.Contains(fixture.PostingBatches, x => x.Status == PostingBatchStatusConst.REVERSAL);
        Assert.Equal(4, fixture.AccountingEntries.Count);
    }
}

file sealed class FaDisposalFixture
{
    public required FaDisposalLifecycleService LifecycleService { get; init; }
    public required FaDisposalDoc Doc { get; init; }
    public required FaAsset Asset { get; init; }
    public required List<PostingBatch> PostingBatches { get; init; }
    public required List<AccountingRegisterEntry> AccountingEntries { get; init; }

    public static FaDisposalFixture Create()
    {
        var asset = FaDisposalTestData.BuildAsset();
        var doc = new FaDisposalDoc
        {
            Id = 1,
            OrganizationId = 8,
            StateId = StateIdConst.ACTIVE,
            DocNumber = "FADS-2026-000001",
            DisposalDate = new DateTime(2026, 8, 1),
            StatusId = DocumentStatusIdConst.DRAFT,
            DisposalType = "SALE",
            CreatedDate = DateTime.Today,
            UpdatedDate = DateTime.Today,
            Lines =
            [
                new FaDisposalDocLine
                {
                    Id = 1,
                    DisposalDocId = 1,
                    FaAssetId = asset.Id,
                    FaAsset = asset,
                    SaleAmount = 500m
                }
            ]
        };
        doc.Lines.Single().DisposalDoc = doc;

        var postingBatches = new List<PostingBatch>();
        var accountingEntries = new List<AccountingRegisterEntry>();
        var depreciationLines = new List<FaDepreciationRunLine>
        {
            new()
            {
                Id = 1,
                DepreciationRunId = 1,
                FaAssetId = asset.Id,
                Amount = 600m,
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

        var service = new FaDisposalLifecycleService(
            new FakeFaDisposalUserContext(),
            new FakeFaDisposalUnitOfWork(),
            new FaDisposalQueryBuilder(),
            new FakeFaDisposalPostingLock(),
            new FakeFaDisposalAccountingPeriodValidator(),
            new FakeFaDisposalAuditLogService(),
            new FakeFaDisposalAccountingDispatcher(accountingEntries, postingBatches),
            new FaDisposalQueryRepository<FaDisposalDoc>(new List<FaDisposalDoc> { doc }),
            new FakeFaDisposalCommandRepository(new List<FaDisposalDoc> { doc }),
            new FakeFaDisposalAssetCommandRepository(new List<FaAsset> { asset }),
            new FaDisposalQueryRepository<FaDepreciationRunLine>(depreciationLines),
            new FaDisposalQueryRepository<PostingBatch>(postingBatches),
            new FakeFaDisposalPostingBatchCommandRepository(postingBatches),
            new FaDisposalQueryRepository<AccountingRegisterEntry>(accountingEntries),
            new FakeFaDisposalAccountingEntryCommandRepository(accountingEntries),
            NullLogger<FaDisposalLifecycleService>.Instance);

        return new FaDisposalFixture
        {
            LifecycleService = service,
            Doc = doc,
            Asset = asset,
            PostingBatches = postingBatches,
            AccountingEntries = accountingEntries
        };
    }
}

file static class FaDisposalTestData
{
    public static FaAsset BuildAsset() => new()
    {
        Id = 1000,
        OrganizationId = 8,
        StateId = StateIdConst.ACTIVE,
        InventoryNumber = "FA-1000",
        Name = "Truck",
        FaGroupId = 1,
        DepreciationMethodId = 1,
        UsefulLifeMonths = 12,
        InitialCost = 1_000m,
        SalvageValue = 0,
        StatusId = FaAssetStatusIdConst.ACTIVE,
        CreatedDate = DateTime.Today,
        UpdatedDate = DateTime.Today,
        Status = new FaAssetStatus { Id = FaAssetStatusIdConst.ACTIVE, Code = "ACTIVE", Name = "Active", StateId = StateIdConst.ACTIVE }
    };
}

file sealed class FakeFaDisposalUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class FakeFaDisposalUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeFaDisposalPostingLock : IDocumentPostingLock
{
    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.FromResult(true);
}

file sealed class FakeFaDisposalAccountingPeriodValidator : IAccountingPeriodValidator
{
    public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class FakeFaDisposalAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class FakeFaDisposalCommandRepository : IFaDisposalCommandRepository
{
    private readonly List<FaDisposalDoc> _docs;

    public FakeFaDisposalCommandRepository(List<FaDisposalDoc> docs)
    {
        _docs = docs;
    }

    public Task CreateAsync(FaDisposalDoc entity, CancellationToken ct = default)
    {
        _docs.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(FaDisposalDoc entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteLinesAsync(IEnumerable<FaDisposalDocLine> entities, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeFaDisposalAssetCommandRepository : IFaAssetCommandRepository
{
    private readonly List<FaAsset> _assets;

    public FakeFaDisposalAssetCommandRepository(List<FaAsset> assets)
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

file sealed class FakeFaDisposalPostingBatchCommandRepository : ICommandRepository<PostingBatch>
{
    private readonly List<PostingBatch> _batches;

    public FakeFaDisposalPostingBatchCommandRepository(List<PostingBatch> batches)
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

file sealed class FakeFaDisposalAccountingEntryCommandRepository : ICommandRepository<AccountingRegisterEntry>
{
    private readonly List<AccountingRegisterEntry> _entries;

    public FakeFaDisposalAccountingEntryCommandRepository(List<AccountingRegisterEntry> entries)
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

file sealed class FakeFaDisposalAccountingDispatcher : IAccountingDispatcher
{
    private readonly List<AccountingRegisterEntry> _entries;
    private readonly List<PostingBatch> _batches;

    public FakeFaDisposalAccountingDispatcher(List<AccountingRegisterEntry> entries, List<PostingBatch> batches)
    {
        _entries = entries;
        _batches = batches;
    }

    public Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null)
    {
        var doc = (FaDisposalDoc)document;
        var batchId = postingBatchId ?? _batches.Single().Id;
        var line = doc.Lines.Single();
        var accumulated = line.FaAsset.InitialCost - line.BookValue;
        var loss = Math.Max(0m, line.BookValue - line.SaleAmount);
        var list = new List<AccountingRegisterEntry>
        {
            new()
            {
                Id = _entries.Count + 1,
                OrganizationId = doc.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.FADISPOSAL,
                DocumentId = doc.Id,
                DebitAccountId = 290,
                CreditAccountId = 190,
                CurrencyId = CurrencyIdConst.UZS,
                Amount = accumulated,
                DocDate = doc.DisposalDate,
                CreatedDate = DateTime.Now,
                PostingBatchId = batchId
            }
        };

        if (line.SaleAmount > 0)
        {
            list.Add(new AccountingRegisterEntry
            {
                Id = _entries.Count + list.Count + 1,
                OrganizationId = doc.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.FADISPOSAL,
                DocumentId = doc.Id,
                DebitAccountId = 5110,
                CreditAccountId = 90301,
                CurrencyId = CurrencyIdConst.UZS,
                Amount = line.SaleAmount,
                DocDate = doc.DisposalDate,
                CreatedDate = DateTime.Now,
                PostingBatchId = batchId
            });
        }

        if (loss > 0)
        {
            list.Add(new AccountingRegisterEntry
            {
                Id = _entries.Count + list.Count + 1,
                OrganizationId = doc.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.FADISPOSAL,
                DocumentId = doc.Id,
                DebitAccountId = 9430,
                CreditAccountId = 190,
                CurrencyId = CurrencyIdConst.UZS,
                Amount = loss,
                DocDate = doc.DisposalDate,
                CreatedDate = DateTime.Now,
                PostingBatchId = batchId
            });
        }

        _entries.AddRange(list);
        return Task.FromResult(Result.Success(list));
    }
}

file sealed class FaDisposalAccountingPolicyResolver : IOrganizationAccountingPolicyResolver
{
    public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) => Task.FromResult(AccountingPolicyIdConst.STANDARD_UZ);
}

file sealed class FaDisposalQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public FaDisposalQueryRepository(IEnumerable<TEntity> data)
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

file sealed class FaDisposalQueryBuilder : IQueryBuilder
{
    private static readonly FaDisposalQueryBuilderResolver Resolver = new();

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

file sealed class FaDisposalQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new FaDisposalProjectionBuilder<TEntity, TResult>();
}

file sealed class FaDisposalProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
