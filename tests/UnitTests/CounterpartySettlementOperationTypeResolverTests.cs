using Application.Abstractions;
using Application.Features.CounterpartyRegisterBalances;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Filters;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public class CounterpartySettlementOperationTypeResolverTests
{
    [Fact]
    public async Task BankPost_ShouldDecreaseDebt_ForSupplierPaymentOut()
    {
        var created = new List<CounterpartyRegisterBalance>();
        var service = new BankCounterpartyRegisterService(
            new SettlementTestQueryBuilder(),
            new SettlementQueryRepository<CounterpartyRegisterBalance>(created),
            new SettlementCommandRepository<CounterpartyRegisterBalance>(created));

        var result = await service.PostAsync(
            new BankOperation
            {
                Id = 100,
                OrganizationId = 8,
                CounterpartyId = 18,
                CurrencyId = 1,
                Amount = 1250m,
                DocDate = new DateTime(2026, 7, 3),
                OperationTypeId = OperationTypeIdConst.OUT,
                PaymentPurpose = new PaymentPurpose
                {
                    Alias = new PostingAlias { Code = AliasConst.Supplier }
                }
            },
            77);

        Assert.True(result.IsSuccess);
        Assert.Single(created);
        Assert.Equal(OperationTypeIdConst.DEBT_DECREASE, created[0].OperationTypeId);
    }

    [Fact]
    public async Task BankPost_ShouldIncreaseOutstanding_ForCustomerAdvanceIn()
    {
        var created = new List<CounterpartyRegisterBalance>();
        var service = new BankCounterpartyRegisterService(
            new SettlementTestQueryBuilder(),
            new SettlementQueryRepository<CounterpartyRegisterBalance>(created),
            new SettlementCommandRepository<CounterpartyRegisterBalance>(created));

        var result = await service.PostAsync(
            new BankOperation
            {
                Id = 101,
                OrganizationId = 8,
                CounterpartyId = 18,
                CurrencyId = 1,
                Amount = 900m,
                DocDate = new DateTime(2026, 7, 3),
                OperationTypeId = OperationTypeIdConst.IN,
                PaymentPurpose = new PaymentPurpose
                {
                    Alias = new PostingAlias { Code = AliasConst.CustomerAdvance }
                }
            },
            78);

        Assert.True(result.IsSuccess);
        Assert.Single(created);
        Assert.Equal(OperationTypeIdConst.DEBT_INCREASE, created[0].OperationTypeId);
    }

    [Fact]
    public async Task CashPost_ShouldDecreaseDebt_ForCustomerSettlementIn()
    {
        var created = new List<CounterpartyRegisterBalance>();
        var service = new CashCounterpartyRegisterService(
            new SettlementTestQueryBuilder(),
            new SettlementQueryRepository<CounterpartyRegisterBalance>(created),
            new SettlementCommandRepository<CounterpartyRegisterBalance>(created));

        var result = await service.PostAsync(
            new CashOperation
            {
                Id = 102,
                OrganizationId = 8,
                CounterpartyId = 18,
                CurrencyId = 1,
                Amount = 500m,
                DocDate = new DateTime(2026, 7, 3),
                OperationTypeId = OperationTypeIdConst.IN,
                PaymentPurpose = new PaymentPurpose
                {
                    Alias = new PostingAlias { Code = AliasConst.Customer }
                }
            },
            79);

        Assert.True(result.IsSuccess);
        Assert.Single(created);
        Assert.Equal(OperationTypeIdConst.DEBT_DECREASE, created[0].OperationTypeId);
    }

    [Fact]
    public async Task CashPost_ShouldIncreaseOutstanding_ForSupplierAdvanceOut()
    {
        var created = new List<CounterpartyRegisterBalance>();
        var service = new CashCounterpartyRegisterService(
            new SettlementTestQueryBuilder(),
            new SettlementQueryRepository<CounterpartyRegisterBalance>(created),
            new SettlementCommandRepository<CounterpartyRegisterBalance>(created));

        var result = await service.PostAsync(
            new CashOperation
            {
                Id = 103,
                OrganizationId = 8,
                CounterpartyId = 18,
                CurrencyId = 1,
                Amount = 650m,
                DocDate = new DateTime(2026, 7, 3),
                OperationTypeId = OperationTypeIdConst.OUT,
                PaymentPurpose = new PaymentPurpose
                {
                    Alias = new PostingAlias { Code = AliasConst.SupplierAdvance }
                }
            },
            80);

        Assert.True(result.IsSuccess);
        Assert.Single(created);
        Assert.Equal(OperationTypeIdConst.DEBT_INCREASE, created[0].OperationTypeId);
    }
}

file sealed class SettlementTestQueryBuilder : IQueryBuilder
{
    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = new SettlementResolver() });

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

file sealed class SettlementResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;

    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() =>
        throw new NotSupportedException();
}

file sealed class SettlementQueryRepository<TEntity>(List<TEntity> items) : IQueryRepository<TEntity>
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
        throw new NotSupportedException();

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        throw new NotSupportedException();
}

file sealed class SettlementCommandRepository<TEntity>(List<TEntity> items) : ICommandRepository<TEntity>
    where TEntity : class
{
    public Task CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        items.Add(entity);
        return Task.CompletedTask;
    }

    public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        items.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
}
