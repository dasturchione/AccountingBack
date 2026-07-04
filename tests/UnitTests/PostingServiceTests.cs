using Application.Abstractions;
using Application.Features.Register;
using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Constants;
using SharedKernel.Filters;
using System.Linq.Expressions;

namespace UnitTests;

public class PostingServiceTests
{
    [Fact]
    public async Task BuildEntriesAsync_ShouldUseContextDocumentType()
    {
        var service = CreateService(
            [
                CreateRule(5, ("PaymentAccount", "Customer"))
            ],
            [
                Resolve("PaymentAccount", 1027),
                Resolve("Customer", 1015)
            ]);

        var entries = await service.BuildEntriesAsync(
        [
            new PostingContext
            {
                OrganizationId = 1,
                DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
                RuleId = 5,
                DocumentId = 12,
                CurrencyId = 1,
                DocDate = DateTime.Today,
                Amounts = new Dictionary<string, decimal> { [AmountSourceConst.Total] = 100m },
                AllowedAliases = [AliasConst.Customer]
            }
        ]);

        Assert.Single(entries);
        Assert.Equal(DocumentTypeIdConst.BANKOPERATION, entries[0].DocumentTypeId);
    }

    [Fact]
    public async Task BuildEntriesAsync_ShouldSkipOptionalAliasesOutsideAllowedSet()
    {
        var service = CreateService(
            [
                CreateRule(5,
                    ("PaymentAccount", "Customer"),
                    ("PaymentAccount", "Supplier"))
            ],
            [
                Resolve("PaymentAccount", 1027),
                Resolve("Customer", 1015),
                Resolve("Supplier", 1036)
            ]);

        var entries = await service.BuildEntriesAsync(
        [
            new PostingContext
            {
                OrganizationId = 1,
                DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
                RuleId = 5,
                DocumentId = 13,
                CurrencyId = 1,
                DocDate = DateTime.Today,
                Amounts = new Dictionary<string, decimal> { [AmountSourceConst.Total] = 200m },
                AllowedAliases = [AliasConst.Customer]
            }
        ]);

        Assert.Single(entries);
        Assert.Equal(1015, entries[0].CreditAccountId);
    }

    [Fact]
    public async Task BuildEntriesAsync_ShouldSelectCashInTransitOptionalLine()
    {
        var service = CreateService(
            [
                CreateRule(5,
                    ("PaymentAccount", "Customer"),
                    ("PaymentAccount", "CashInTransit"))
            ],
            [
                Resolve("PaymentAccount", 1027),
                Resolve("Customer", 1015),
                Resolve("CashInTransit", 1073)
            ]);

        var entries = await service.BuildEntriesAsync(
        [
            new PostingContext
            {
                OrganizationId = 1,
                DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
                RuleId = 5,
                DocumentId = 14,
                CurrencyId = 1,
                DocDate = DateTime.Today,
                Amounts = new Dictionary<string, decimal> { [AmountSourceConst.Total] = 350m },
                AllowedAliases = [AliasConst.CashInTransit]
            }
        ]);

        Assert.Single(entries);
        Assert.Equal(1073, entries[0].CreditAccountId);
    }

    [Fact]
    public async Task BuildEntriesAsync_ShouldResolveVatInByVatKind_ToPostableLeafAccount()
    {
        // VATIn on a group account (4410) must be resolved to the postable leaf
        // subaccount for the purchase kind: goods → 4410.3, services → 4410.4.
        var service = CreateService(
            [
                CreateRule(PostingRuleIdConst.PURCHASE_GOODS, ("VATIn", "Supplier"))
            ],
            [
                ResolveDim("VATIn", 1020, RegisterDefaultsConst.VatKindGoods),
                ResolveDim("VATIn", 1021, RegisterDefaultsConst.VatKindServices),
                ResolveDim("VATIn", 1020, RegisterDefaultsConst.DefaultDimensionValue),
                Resolve("Supplier", 1036)
            ]);

        var goodsEntries = await service.BuildEntriesAsync(
        [
            new PostingContext
            {
                OrganizationId = 1,
                DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                RuleId = PostingRuleIdConst.PURCHASE_GOODS,
                DocumentId = 20,
                CurrencyId = 1,
                DocDate = DateTime.Today,
                VatKind = RegisterDefaultsConst.VatKindGoods,
                Amounts = new Dictionary<string, decimal> { [AmountSourceConst.Total] = 120m }
            }
        ]);

        var servicesEntries = await service.BuildEntriesAsync(
        [
            new PostingContext
            {
                OrganizationId = 1,
                DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                RuleId = PostingRuleIdConst.PURCHASE_GOODS,
                DocumentId = 21,
                CurrencyId = 1,
                DocDate = DateTime.Today,
                VatKind = RegisterDefaultsConst.VatKindServices,
                Amounts = new Dictionary<string, decimal> { [AmountSourceConst.Total] = 120m }
            }
        ]);

        Assert.Equal(1020, goodsEntries.Single().DebitAccountId);
        Assert.Equal(1021, servicesEntries.Single().DebitAccountId);
    }

    private static PostingService CreateService(
        List<PostingRule> postingRules,
        List<AccountResolveRule> resolveRules)
    {
        return new PostingService(
            new FakeQueryBuilder(),
            new FakeQueryRepository<PostingRule>(postingRules),
            new FakeQueryRepository<AccountResolveRule>(resolveRules),
            new FakeQueryRepository<ChartAccount>([]));
    }

    private static PostingRule CreateRule(short id, params (string DebitAlias, string CreditAlias)[] lines) =>
        new()
        {
            Id = id,
            Code = $"RULE_{id}",
            Name = $"Rule {id}",
            PostingRuleLines = lines.Select((line, index) => new PostingRuleLine
            {
                Id = index + 1,
                TemplateId = id,
                OrderNumber = (short)(index + 1),
                DebitAlias = new PostingAlias { Code = line.DebitAlias },
                CreditAlias = new PostingAlias { Code = line.CreditAlias },
                AmountSource = AmountSourceConst.Total,
                IsOptional = true
            }).ToList()
        };

    private static AccountResolveRule Resolve(string alias, int accountId) =>
        ResolveDim(alias, accountId, "_default");

    private static AccountResolveRule ResolveDim(string alias, int accountId, string dimensionValue) =>
        new()
        {
            Alias = alias,
            AccountId = accountId,
            DimensionValue = dimensionValue,
            Priority = 1
        };

    private sealed class FakeQueryBuilder : IQueryBuilder
    {
        public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
            new(new QueryState<TEntity> { Resolver = new FakeResolver() });

        public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class =>
            throw new NotSupportedException();

        public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class =>
            throw new NotSupportedException();

        public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options)
            where TEntity : class
            where TOptions : IPaginationFilter =>
            throw new NotSupportedException();

        public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options)
            where TEntity : class
            where TOptions : IPaginationFilter =>
            throw new NotSupportedException();
    }

    private sealed class FakeResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;

        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() =>
            throw new NotSupportedException();
    }

    private sealed class FakeQueryRepository<TEntity>(List<TEntity> items) : IQueryRepository<TEntity>
        where TEntity : class
    {
        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Any(predicate));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().FirstOrDefault(specification.Criteria));

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Where(specification.Criteria).ToList());

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Where(specification.Criteria).Select(specification.Selector).ToList());

        public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }
}
