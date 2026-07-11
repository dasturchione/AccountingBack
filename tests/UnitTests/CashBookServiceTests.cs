using Application.Abstractions.Authentication;
using Application.Abstractions;
using Application.Features.CashDocuments;
using Domain.Entities;
using SharedKernel.Filters;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public class CashBookServiceTests
{
    [Fact]
    public async Task GetAsync_ShouldCalculateRunningBalance_FromPageOpeningBalance()
    {
        var service = new CashBookService(
            new CashBookUserContext(),
            new CashBookQueryBuilder(),
            new CashBookQueryRepository<CashBox>(
            [
                new CashBox { Id = 4, Name = "Main cash", StateId = 1, OrganizationId = 8, CurrencyId = 1, CreatedDate = DateTime.Today, OpeningBalance = 100m }
            ]),
            new FakeCashBookReadRepository());

        var result = await service.GetAsync(new CashBookFilter
        {
            CashBoxId = 4,
            Page = 1,
            PageSize = 50
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(150m, result.Value.Items.First().RunningBalance);
        Assert.Equal(120m, result.Value.Items.Last().RunningBalance);
    }
}

file sealed class FakeCashBookReadRepository : ICashBookReadRepository
{
    public Task<CashBookReadResult> GetAsync(CashBookReadRequest request, CancellationToken ct = default) =>
        Task.FromResult(new CashBookReadResult
        {
            OpeningBalance = 100m,
            ClosingBalance = 120m,
            PageOpeningBalance = 100m,
            TotalReceipt = 50m,
            TotalPayment = 30m,
            TotalCount = 2,
            Entries =
            [
                new CashBookReadEntry
                {
                    MoneyRegisterEntryId = 1,
                    CashOperationId = 10,
                    DocDate = new DateTime(2026, 7, 3),
                    DocNumber = "CASH-1",
                    DocumentKind = "PKO",
                    CurrencyId = 1,
                    CurrencyName = "UZS",
                    Receipt = 50m,
                    Payment = 0m
                },
                new CashBookReadEntry
                {
                    MoneyRegisterEntryId = 2,
                    CashOperationId = 11,
                    DocDate = new DateTime(2026, 7, 3),
                    DocNumber = "CASH-2",
                    DocumentKind = "RKO",
                    CurrencyId = 1,
                    CurrencyName = "UZS",
                    Receipt = 0m,
                    Payment = 30m
                }
            ]
        });
}

file sealed class CashBookUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class CashBookQueryBuilder : IQueryBuilder
{
    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = new CashBookQueryBuilderResolver() });

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

file sealed class CashBookQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;

    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() =>
        throw new NotSupportedException();
}

file sealed class CashBookQueryRepository<TEntity>(IEnumerable<TEntity> items) : IQueryRepository<TEntity>
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
