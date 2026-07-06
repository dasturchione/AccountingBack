using Application.Abstractions;
using Application.Features.Register;
using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using System.Linq.Expressions;

namespace UnitTests;

public class FaReceiptConfirmPostingBalanceTests
{
    [Fact]
    public async Task FaReceiptPosting_ShouldFollow1CScheme_ForConfirm()
    {
        // fa_receipt: 1 строка, 1 актив стоимостью 1 000 000 + НДС 120 000, поставщик 18.
        var doc = new FaReceiptDoc
        {
            Id = 55,
            OrganizationId = 8,
            DocNumber = "FA-55",
            DocDate = new DateTime(2026, 7, 6, 10, 0, 0),
            CounterpartyId = 18,
            CurrencyId = 1,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            TotalAmount = 1_000_000m,
            VatAmount = 120_000m,
            FinalAmount = 1_120_000m,
            ReceiptType = FaReceiptTypeConst.PURCHASE,
            CreatedDate = DateTime.Today,
            Lines =
            [
                new FaReceiptDocLine
                {
                    Id = 1,
                    OwnerId = 55,
                    Name = "Excavator",
                    Quantity = 1,
                    Price = 1_000_000m,
                    Amount = 1_000_000m,
                    VatAmount = 120_000m,
                    TotalAmount = 1_120_000m,
                    Assets =
                    [
                        new FaReceiptDocAsset
                        {
                            Id = 1,
                            OwnerId = 1,
                            FaAssetId = 777,
                            InventoryNumber = "INV-777",
                            Name = "Excavator",
                            InitialCost = 1_000_000m,
                            FaGroupId = 1,
                            DepreciationMethodId = 1,
                            UsefulLifeMonths = 60
                        }
                    ]
                }
            ]
        };

        var queryBuilder = new FaPostingQueryBuilder();
        var builder = new FaReceiptContextBuilder(
            new FaPostingAccountingPolicyResolver(),
            queryBuilder,
            new FaPostingQueryRepository<CounterpartyCard>(
            [
                new CounterpartyCard { Id = 18, FullName = "Vendor", ShortName = "Vendor", OrganizationId = 8, CounterpartyTypeId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
            ]));

        var contexts = await builder.BuildAsync(doc);

        var rule = BuildFaReceiptRule();

        var postingService = new PostingService(
            queryBuilder,
            new FaPostingQueryRepository<PostingRule>([rule]),
            new FaPostingQueryRepository<AccountResolveRule>(
            [
                new AccountResolveRule { Id = 1, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAssetInProgress, DimensionKey = "_none", DimensionValue = "_default", AccountId = 820, Priority = 100 },
                new AccountResolveRule { Id = 2, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.FixedAsset, DimensionKey = "_none", DimensionValue = "_default", AccountId = 190, Priority = 100 },
                new AccountResolveRule { Id = 3, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.Supplier, DimensionKey = "_none", DimensionValue = "_default", AccountId = 6010, Priority = 100 },
                new AccountResolveRule { Id = 4, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.VATIn, DimensionKey = "vatKind", DimensionValue = RegisterDefaultsConst.VatKindFixedAsset, AccountId = 4411, Priority = 10 }
            ]),
            new FaPostingQueryRepository<ChartAccount>(
            [
                new ChartAccount { Id = 820, Code = "0820", Name = "Acquisition of FA", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 190, Code = "0190", Name = "Other FA", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 6010, Code = "6010", Name = "Supplier", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 4411, Code = "4410.1", Name = "Input VAT FA", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
            ]));

        var entries = await postingService.BuildEntriesAsync(contexts);
        var validation = new AccountingPostingValidator().Validate(entries);

        Assert.True(validation.IsSuccess);
        Assert.Equal(3, entries.Count);

        // Dr 0800 → Cr 6010 (капитализация)
        Assert.Single(entries, x => x.DebitAccountId == 820 && x.CreditAccountId == 6010 && x.Amount == 1_000_000m);
        // Dr 4410.1 → Cr 6010 (входной НДС ОС)
        Assert.Single(entries, x => x.DebitAccountId == 4411 && x.CreditAccountId == 6010 && x.Amount == 120_000m);
        // Dr 0100 → Cr 0800 (ввод в эксплуатацию)
        Assert.Single(entries, x => x.DebitAccountId == 190 && x.CreditAccountId == 820 && x.Amount == 1_000_000m);

        // Итог по кредиту 6010 = стоимость + НДС.
        Assert.Equal(1_120_000m, entries.Where(x => x.CreditAccountId == 6010).Sum(x => x.Amount));

        // Субконто fixed_asset прикреплено к счетам ОС/капвложений.
        var assetSubkontos = entries
            .SelectMany(x => x.RegisterEntrySubkontos)
            .Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.FIXED_ASSET)
            .ToList();
        Assert.NotEmpty(assetSubkontos);
        Assert.All(assetSubkontos, x => Assert.Equal(777, x.EntityId));
    }

    private static PostingRule BuildFaReceiptRule()
    {
        var fixedAssetInProgress = new PostingAlias { Id = 36, Code = AliasConst.FixedAssetInProgress, Name = "Capex" };
        var supplier = new PostingAlias { Id = 3, Code = AliasConst.Supplier, Name = "Supplier" };
        var vatIn = new PostingAlias { Id = 9, Code = AliasConst.VATIn, Name = "VAT In" };
        var fixedAsset = new PostingAlias { Id = 35, Code = AliasConst.FixedAsset, Name = "Fixed asset" };

        return new PostingRule
        {
            Id = PostingRuleIdConst.FA_RECEIPT,
            Code = "FA_RECEIPT",
            Name = "Fixed asset receipt",
            PostingRuleLines =
            [
                new PostingRuleLine
                {
                    Id = 25, TemplateId = PostingRuleIdConst.FA_RECEIPT, OrderNumber = 1,
                    DebitAliasId = fixedAssetInProgress.Id, CreditAliasId = supplier.Id,
                    AmountSource = AmountSourceConst.Base, IsOptional = false,
                    DebitAlias = fixedAssetInProgress, CreditAlias = supplier
                },
                new PostingRuleLine
                {
                    Id = 26, TemplateId = PostingRuleIdConst.FA_RECEIPT, OrderNumber = 2,
                    DebitAliasId = vatIn.Id, CreditAliasId = supplier.Id,
                    AmountSource = AmountSourceConst.VAT, IsOptional = true,
                    DebitAlias = vatIn, CreditAlias = supplier
                },
                new PostingRuleLine
                {
                    Id = 27, TemplateId = PostingRuleIdConst.FA_RECEIPT, OrderNumber = 3,
                    DebitAliasId = fixedAsset.Id, CreditAliasId = fixedAssetInProgress.Id,
                    AmountSource = AmountSourceConst.Base, IsOptional = false,
                    DebitAlias = fixedAsset, CreditAlias = fixedAssetInProgress
                }
            ]
        };
    }
}

file sealed class FaPostingAccountingPolicyResolver : IOrganizationAccountingPolicyResolver
{
    public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) =>
        Task.FromResult(AccountingPolicyIdConst.STANDARD_UZ);
}

file sealed class FaPostingQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public FaPostingQueryRepository(IEnumerable<TEntity> data)
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

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();
}

file sealed class FaPostingQueryBuilder : IQueryBuilder
{
    private static readonly FaPostingQueryBuilderResolver Resolver = new();

    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = Resolver });

    public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true };

    public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default! };

    public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : SharedKernel.Filters.IPaginationFilter =>
        throw new NotSupportedException();

    public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : SharedKernel.Filters.IPaginationFilter =>
        throw new NotSupportedException();
}

file sealed class FaPostingQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new FaPostingProjectionBuilder<TEntity, TResult>();
}

file sealed class FaPostingProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
