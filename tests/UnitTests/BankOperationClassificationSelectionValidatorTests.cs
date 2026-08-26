using System.Linq.Expressions;
using Application.Abstractions;
using Application.Features.BankOperations;
using Domain.Entities;
using Infrastructure.Query;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;

namespace UnitTests;

public sealed class BankOperationClassificationSelectionValidatorTests
{
    [Fact]
    public async Task ValidateAsync_RejectsRuleFromAnotherBank()
    {
        var validator = new BankOperationClassificationSelectionValidator(
            new InMemoryQueryRepository<BankAccount>(new BankAccount
            {
                Id = 10,
                OrganizationId = 7,
                BankId = 2,
                StateId = StateIdConst.ACTIVE
            }),
            new InMemoryQueryRepository<BankOperationCategory>(new BankOperationCategory
            {
                Id = 3,
                Code = "ACQUIRING",
                Name = "Acquiring",
                StateId = StateIdConst.ACTIVE
            }),
            new InMemoryQueryRepository<BankOperationClassificationRule>(new BankOperationClassificationRule
            {
                Id = 15,
                CategoryId = 3,
                StateId = StateIdConst.ACTIVE,
                RuleSet = new BankOperationClassificationRuleSet
                {
                    BankId = 99,
                    StateId = StateIdConst.ACTIVE
                }
            }),
            new QueryBuilder(new NullQueryBuilderResolver()));

        var result = await validator.ValidateAsync(7, 10, 3, 15);

        Assert.False(result.IsSuccess);
        Assert.Equal("BankOperation.ClassificationBankMismatch", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAsync_AllowsManualCategoryWithoutRule()
    {
        var validator = new BankOperationClassificationSelectionValidator(
            new InMemoryQueryRepository<BankAccount>(new BankAccount
            {
                Id = 10,
                OrganizationId = 7,
                BankId = 2,
                StateId = StateIdConst.ACTIVE
            }),
            new InMemoryQueryRepository<BankOperationCategory>(new BankOperationCategory
            {
                Id = 3,
                Code = "ACQUIRING",
                Name = "Acquiring",
                StateId = StateIdConst.ACTIVE
            }),
            new InMemoryQueryRepository<BankOperationClassificationRule>(),
            new QueryBuilder(new NullQueryBuilderResolver()));

        var result = await validator.ValidateAsync(7, 10, 3, null);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAsync_AllowsRuleFromSelectedCategoryAndBank()
    {
        var validator = new BankOperationClassificationSelectionValidator(
            new InMemoryQueryRepository<BankAccount>(new BankAccount
            {
                Id = 10,
                OrganizationId = 7,
                BankId = 2,
                StateId = StateIdConst.ACTIVE
            }),
            new InMemoryQueryRepository<BankOperationCategory>(new BankOperationCategory
            {
                Id = 3,
                Code = "ACQUIRING",
                Name = "Acquiring",
                StateId = StateIdConst.ACTIVE
            }),
            new InMemoryQueryRepository<BankOperationClassificationRule>(new BankOperationClassificationRule
            {
                Id = 15,
                CategoryId = 3,
                StateId = StateIdConst.ACTIVE,
                RuleSet = new BankOperationClassificationRuleSet
                {
                    BankId = 2,
                    StateId = StateIdConst.ACTIVE
                }
            }),
            new QueryBuilder(new NullQueryBuilderResolver()));

        var result = await validator.ValidateAsync(7, 10, 3, 15);

        Assert.True(result.IsSuccess);
    }

    private sealed class InMemoryQueryRepository<TEntity>(params TEntity[] entities)
        : IQueryRepository<TEntity> where TEntity : class
    {
        private readonly List<TEntity> _entities = [.. entities];
        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.FromResult(_entities.AsQueryable().Any(predicate));
        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) => Task.FromResult(_entities.AsQueryable().FirstOrDefault(specification.Criteria));
        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) => Task.FromResult(_entities.AsQueryable().Where(specification.Criteria).Select(specification.Selector).FirstOrDefault(specification.ResultCriteria));
        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) => Task.FromResult(_entities.AsQueryable().Where(specification.Criteria).ToList());
        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) => Task.FromResult(_entities.AsQueryable().Where(specification.Criteria).Select(specification.Selector).Where(specification.ResultCriteria).ToList());
        public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class NullQueryBuilderResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => throw new NotSupportedException();
        public IOrderByBuilder<TEntity, TResult>? GetOrderByBuilder<TEntity, TResult>() => null;
    }
}
