using System.Linq.Expressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.BankParsers;
using Domain.Entities;
using Infrastructure.Query;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;

namespace UnitTests;

public sealed class BankOperationClassifierTests
{
    [Fact]
    public async Task ClassifyAsync_AddsRuleResultToTransactions()
    {
        var fallback = new BankOperationClassificationRule
        {
            Id = 51,
            Code = "COUNTERPARTY",
            Priority = 999,
            IsFallback = true,
            StateId = StateIdConst.ACTIVE,
            CategoryId = 7,
            Category = new BankOperationCategory
            {
                Id = 7,
                Code = "COUNTERPARTY",
                Name = "Counterparty",
                StateId = StateIdConst.ACTIVE,
                Translations =
                [
                    new BankOperationCategoryTranslation { LanguageId = LanguageIdConst.RU, Name = "Контрагент" }
                ]
            }
        };
        var ruleSet = new BankOperationClassificationRuleSet
        {
            Id = 1,
            BankId = 2,
            Version = 1,
            StateId = StateIdConst.ACTIVE,
            Rules = [fallback]
        };
        var service = CreateService(ruleSet, new Organization { Id = 7, Inn = "311444422" });
        var export = new BankExportDto
        {
            Accounts =
            [
                new AccountStatementDto
                {
                    CompanyInn = "311 444 422",
                    Transactions = [new TransactionDto { Purpose = "Payment" }]
                }
            ]
        };

        var result = await service.ClassifyAsync(export, 2);

        Assert.True(result.IsSuccess);
        var transaction = Assert.Single(Assert.Single(export.Accounts).Transactions);
        Assert.Equal((short)7, transaction.ClassificationCategoryId);
        Assert.Equal("COUNTERPARTY", transaction.ClassificationCode);
        Assert.Equal("Контрагент", transaction.ClassificationName);
        Assert.Equal(51, transaction.ClassificationRuleId);
        Assert.Equal("COUNTERPARTY", transaction.ClassificationRuleCode);
        Assert.False(transaction.RequiresReview);
    }

    [Fact]
    public async Task ClassifyAsync_RejectsStatementForAnotherOrganization()
    {
        var service = CreateService(
            new BankOperationClassificationRuleSet
            {
                Id = 1,
                BankId = 2,
                Version = 1,
                StateId = StateIdConst.ACTIVE
            },
            new Organization { Id = 7, Inn = "311444422" });
        var export = new BankExportDto
        {
            Accounts = [new AccountStatementDto { CompanyInn = "999999999" }]
        };

        var result = await service.ClassifyAsync(export, 2);

        Assert.False(result.IsSuccess);
        Assert.Equal("BankStatement.OrganizationMismatch", result.Error.Code);
    }

    [Fact]
    public async Task ClassifyAsync_UsesGlobalReviewCategoryWhenNoRuleMatchesAndNoFallbackExists()
    {
        var category = new BankOperationCategory
        {
            Id = 8,
            Code = "REVIEW_REQUIRED",
            Name = "Review required",
            StateId = StateIdConst.ACTIVE
        };
        var unmatchedRule = new BankOperationClassificationRule
        {
            Id = 10,
            Code = "BANK_COMMISSION",
            Priority = 10,
            DirectionId = MovementDirectionIdConst.OUT,
            StateId = StateIdConst.ACTIVE,
            CategoryId = 1,
            Category = new BankOperationCategory
            {
                Id = 1,
                Code = "BANK_COMMISSION",
                Name = "Bank commission",
                StateId = StateIdConst.ACTIVE
            },
            Conditions =
            [
                new BankOperationClassificationCondition
                {
                    ConditionOrder = 1,
                    FieldCode = "PURPOSE",
                    OperatorCode = "CONTAINS",
                    ValueSourceCode = "LITERAL",
                    CompareValue = "от суммы",
                    NormalizationCode = "NORMALIZE_WHITESPACE"
                }
            ]
        };
        var service = CreateService(
            new BankOperationClassificationRuleSet
            {
                Id = 1,
                BankId = 2,
                Version = 1,
                StateId = StateIdConst.ACTIVE,
                Rules = [unmatchedRule]
            },
            new Organization { Id = 7, Inn = "311444422" },
            category);
        var export = new BankExportDto
        {
            Accounts =
            [
                new AccountStatementDto
                {
                    CompanyInn = "311444422",
                    Transactions = [new TransactionDto { Purpose = "ordinary payment", Credit = 100 }]
                }
            ]
        };

        var result = await service.ClassifyAsync(export, 2);

        Assert.True(result.IsSuccess);
        var transaction = Assert.Single(Assert.Single(export.Accounts).Transactions);
        Assert.Equal((short)8, transaction.ClassificationCategoryId);
        Assert.Equal("REVIEW_REQUIRED", transaction.ClassificationCode);
        Assert.Null(transaction.ClassificationRuleId);
        Assert.Null(transaction.ClassificationRuleCode);
        Assert.True(transaction.RequiresReview);
    }

    private static BankOperationClassifier CreateService(
        BankOperationClassificationRuleSet ruleSet,
        Organization organization,
        params BankOperationCategory[] categories) =>
        new(
            new TestUserContext(),
            new InMemoryQueryRepository<BankOperationClassificationRuleSet>(ruleSet),
            new InMemoryQueryRepository<Organization>(organization),
            new InMemoryQueryRepository<BankOperationCategory>(categories),
            new QueryBuilder(new NullQueryBuilderResolver()));

    private sealed class TestUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => LanguageIdConst.RU;
        public int? TenantId => 1;
        public int? OrganizationId => 7;
        public List<int> AllowedOrganizationIds => [7];
        public int? BranchId => null;
    }

    private sealed class InMemoryQueryRepository<TEntity>(params TEntity[] entities)
        : IQueryRepository<TEntity> where TEntity : class
    {
        private readonly List<TEntity> _entities = [.. entities];

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().Any(predicate));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().FirstOrDefault(specification.Criteria));

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().Where(specification.Criteria).Select(specification.Selector)
                .FirstOrDefault(specification.ResultCriteria));

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().Where(specification.Criteria).ToList());

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().Where(specification.Criteria).Select(specification.Selector)
                .Where(specification.ResultCriteria).ToList());

        public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class NullQueryBuilderResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => throw new NotSupportedException();
        public IOrderByBuilder<TEntity, TResult>? GetOrderByBuilder<TEntity, TResult>() => null;
    }
}
