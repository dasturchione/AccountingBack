using Application.Abstractions;
using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using System.Linq.Expressions;

namespace UnitTests;

public class PurchaseConfirmPostingBalanceTests
{
    [Fact]
    public async Task PurchaseGoodsPosting_ShouldBeBalanced_ForConfirm()
    {
        var product = new Product
        {
            Id = 24,
            Name = "Tracked product",
            UnitId = 1,
            OrganizationId = 8,
            IsService = false,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        };

        var purchase = new PurchaseDoc
        {
            Id = 100,
            OrganizationId = 8,
            DocNumber = "PUR-100",
            DocDate = new DateTime(2026, 7, 3, 15, 59, 16),
            CounterpartyId = 18,
            WarehouseId = 7,
            CurrencyId = 1,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today,
            PurchaseDocProducts =
            [
                new PurchaseDocProduct
                {
                    Id = 1,
                    OwnerId = 100,
                    ProductId = 24,
                    Product = product,
                    UnitId = 1,
                    Quantity = 1,
                    UnitPrice = 10000,
                    Amount = 10000,
                    VatRateId = 2,
                    VatAmount = 1200,
                    TotalAmount = 11200
                }
            ]
        };

        var queryBuilder = new PurchasePostingQueryBuilder();
        var builder = new PurchaseDocContextBuilder(
            new PurchasePostingAccountingPolicyResolver(),
            queryBuilder,
            new PurchasePostingQueryRepository<Product>([product]),
            new PurchasePostingQueryRepository<Contract>([]),
            new PurchasePostingQueryRepository<Warehouse>([new Warehouse { Id = 7, Name = "Main warehouse", OrganizationId = 8, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }]),
            new PurchasePostingQueryRepository<CounterpartyCard>([new CounterpartyCard { Id = 18, FullName = "Vendor", ShortName = "Vendor", OrganizationId = 8, CounterpartyTypeId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }]));

        var contexts = await builder.BuildAsync(purchase);

        var inventoryAlias = new PostingAlias { Id = 1, Code = AliasConst.Inventory, Name = "Inventory" };
        var supplierAlias = new PostingAlias { Id = 2, Code = AliasConst.Supplier, Name = "Supplier" };
        var vatInAlias = new PostingAlias { Id = 3, Code = AliasConst.VATIn, Name = "VAT In" };

        var rule = new PostingRule
        {
            Id = PostingRuleIdConst.PURCHASE_GOODS,
            Code = "PURCHASE_GOODS",
            Name = "Purchase goods",
            PostingRuleLines =
            [
                new PostingRuleLine
                {
                    Id = 1,
                    TemplateId = PostingRuleIdConst.PURCHASE_GOODS,
                    OrderNumber = 1,
                    DebitAliasId = inventoryAlias.Id,
                    CreditAliasId = supplierAlias.Id,
                    AmountSource = AmountSourceConst.Base,
                    IsOptional = false,
                    DebitAlias = inventoryAlias,
                    CreditAlias = supplierAlias
                },
                new PostingRuleLine
                {
                    Id = 2,
                    TemplateId = PostingRuleIdConst.PURCHASE_GOODS,
                    OrderNumber = 2,
                    DebitAliasId = vatInAlias.Id,
                    CreditAliasId = supplierAlias.Id,
                    AmountSource = AmountSourceConst.VAT,
                    IsOptional = false,
                    DebitAlias = vatInAlias,
                    CreditAlias = supplierAlias
                }
            ]
        };

        var postingService = new PostingService(
            queryBuilder,
            new PurchasePostingQueryRepository<PostingRule>([rule]),
            new PurchasePostingQueryRepository<AccountResolveRule>(
            [
                new AccountResolveRule { Id = 1, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.Inventory, DimensionKey = "_none", DimensionValue = "_default", AccountId = 2910, Priority = 100 },
                new AccountResolveRule { Id = 2, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.Supplier, DimensionKey = "_none", DimensionValue = "_default", AccountId = 6010, Priority = 100 },
                new AccountResolveRule { Id = 3, PolicyId = AccountingPolicyIdConst.STANDARD_UZ, Alias = AliasConst.VATIn, DimensionKey = "_none", DimensionValue = "_default", AccountId = 4410, Priority = 100 }
            ]),
            new PurchasePostingQueryRepository<ChartAccount>(
            [
                new ChartAccount { Id = 2910, Code = "2910", Name = "Inventory", IsQuantity = true, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 6010, Code = "6010", Name = "Supplier", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
                new ChartAccount { Id = 4410, Code = "4410", Name = "VAT In", IsQuantity = false, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
            ]));

        var entries = await postingService.BuildEntriesAsync(contexts);
        var validation = new AccountingPostingValidator().Validate(entries);

        Assert.True(validation.IsSuccess);
        Assert.Equal(1m, entries.Sum(x => x.DebitQuantity ?? 0m));
        Assert.Equal(0m, entries.Sum(x => x.CreditQuantity ?? 0m));
        Assert.Single(entries, x => x.DebitAccountId == 2910 && x.DebitQuantity == 1m);
        Assert.All(entries.Where(x => x.CreditAccountId == 6010), x => Assert.Null(x.CreditQuantity));
        Assert.All(entries.Where(x => x.DebitAccountId == 4410), x => Assert.Null(x.DebitQuantity));
        Assert.Equal(11200m, entries.Sum(x => x.Amount));
    }
}

file sealed class PurchasePostingAccountingPolicyResolver : IOrganizationAccountingPolicyResolver
{
    public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) =>
        Task.FromResult(AccountingPolicyIdConst.STANDARD_UZ);
}

file sealed class PurchasePostingQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public PurchasePostingQueryRepository(IEnumerable<TEntity> data)
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

file sealed class PurchasePostingQueryBuilder : IQueryBuilder
{
    private static readonly PurchasePostingQueryBuilderResolver Resolver = new();

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

file sealed class PurchasePostingQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new PurchasePostingProjectionBuilder<TEntity, TResult>();
}

file sealed class PurchasePostingProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
