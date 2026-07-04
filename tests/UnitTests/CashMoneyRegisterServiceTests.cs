using Application.Abstractions;
using Application.Features.Register;
using Application.Features.MoneyRegisterBalances;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Filters;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using System.Linq.Expressions;

namespace UnitTests;

public class CashMoneyRegisterServiceTests
{
    [Fact]
    public async Task GetCashBoxBalanceAsync_ShouldIncludeReversalEntries()
    {
        var service = new CashMoneyRegisterService(
            new CashMoneyQueryBuilder(),
            new CashMoneyQueryRepository<CashBox>(
            [
                new CashBox { Id = 4, OpeningBalance = 100m, OrganizationId = 8, CurrencyId = 1, StateId = StateIdConst.ACTIVE, Name = "Main cash", CreatedDate = DateTime.Today }
            ]),
            new CashMoneyQueryRepository<MoneyRegisterBalance>(
            [
                new MoneyRegisterBalance
                {
                    Id = 1,
                    DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
                    DocumentId = 10,
                    SourceType = RegisterDefaultsConst.CashOperation,
                    SourceId = 4,
                    OperationTypeId = OperationTypeIdConst.IN,
                    Amount = 50m,
                    DocDate = new DateTime(2026, 7, 3),
                    CurrencyId = 1,
                    OrganizationId = 8,
                    CreatedDate = DateTime.Today
                },
                new MoneyRegisterBalance
                {
                    Id = 2,
                    DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
                    DocumentId = 10,
                    SourceType = RegisterDefaultsConst.CashOperation,
                    SourceId = 4,
                    OperationTypeId = OperationTypeIdConst.OUT,
                    Amount = 50m,
                    DocDate = new DateTime(2026, 7, 4),
                    CurrencyId = 1,
                    OrganizationId = 8,
                    CreatedDate = DateTime.Today,
                    ReversalEntryId = 1
                }
            ]),
            new CashMoneyCommandRepository<MoneyRegisterBalance>());

        var balance = await service.GetCashBoxBalanceAsync(4, new DateTime(2026, 7, 5));

        Assert.Equal(100m, balance);
    }
}

file sealed class CashMoneyQueryBuilder : IQueryBuilder
{
    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = new CashMoneyQueryBuilderResolver() });

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

file sealed class CashMoneyQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;

    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() =>
        throw new NotSupportedException();
}

file sealed class CashMoneyQueryRepository<TEntity>(IEnumerable<TEntity> items) : IQueryRepository<TEntity>
    where TEntity : class
{
    private readonly List<TEntity> _items = items.ToList();

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        Task.FromResult(_items.AsQueryable().Any(predicate));

    public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_items.AsQueryable().FirstOrDefault(specification.Criteria));

    public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_items.AsQueryable().Where(specification.Criteria).ToList());

    public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();
}

file sealed class CashMoneyCommandRepository<TEntity> : Application.Abstractions.ICommandRepository<TEntity>
    where TEntity : class
{
    public Task CreateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
}
