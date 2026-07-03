using Application.Abstractions;
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
        new()
        {
            Alias = alias,
            AccountId = accountId,
            DimensionValue = "_default",
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
