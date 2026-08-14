using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Contracts;
using Application.Features.Manual;
using Domain.Entities;
using Infrastructure.Query;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public sealed class ContractFlowTests
{
    [Fact]
    public async Task ActiveContractWithinDateRangeIsReturned()
    {
        var service = CreateService(
            [Contract(1, 10, 7, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31))],
            [Counterparty(7, 10)]);

        var result = await service.GetContractsAsync(7, choosedDate: new DateTime(2026, 6, 1));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(1, result.Value[0].Id);
    }

    [Fact]
    public async Task ExpiredContractIsNotReturned()
    {
        var service = CreateService(
            [Contract(1, 10, 7, new DateTime(2025, 1, 1), new DateTime(2025, 12, 31))],
            [Counterparty(7, 10)]);

        var result = await service.GetContractsAsync(7, choosedDate: new DateTime(2026, 1, 1));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task FutureContractIsNotReturned()
    {
        var service = CreateService(
            [Contract(1, 10, 7, new DateTime(2026, 7, 1), null)],
            [Counterparty(7, 10)]);

        var result = await service.GetContractsAsync(7, choosedDate: new DateTime(2026, 6, 30));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task ActiveContractWithoutEndDateIsReturned()
    {
        var service = CreateService(
            [Contract(1, 10, 7, new DateTime(2026, 1, 1), null)],
            [Counterparty(7, 10)]);

        var result = await service.GetContractsAsync(7, choosedDate: new DateTime(2026, 12, 31));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task DateOutsideContractRangeReturnsEmptyList()
    {
        var service = CreateService(
            [Contract(1, 10, 7, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31))],
            [Counterparty(7, 10)]);

        var result = await service.GetContractsAsync(7, choosedDate: new DateTime(2026, 6, 1));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task CounterpartyFromAnotherOrganizationReturnsEmptyList()
    {
        var service = CreateService(
            [Contract(1, 10, 7, new DateTime(2026, 1, 1), null)],
            [Counterparty(7, 11)]);

        var result = await service.GetContractsAsync(7, choosedDate: new DateTime(2026, 6, 1));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task NoMatchingContractReturnsEmptyListInsteadOfNull()
    {
        var service = CreateService([], [Counterparty(7, 10)]);

        var result = await service.GetContractsAsync(7, choosedDate: new DateTime(2026, 6, 1));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task DefaultDateValueIsAControlledValidationError()
    {
        var service = CreateService([], [Counterparty(7, 10)]);

        var result = await service.GetContractsAsync(7, choosedDate: default(DateTime));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Manual.InvalidContractDate", result.Error.Code);
    }

    [Fact]
    public void ContractListProjectionIncludesCommentAndExistingFields()
    {
        var projection = new ContractListDtoProjection().Build().Compile();
        var entity = Contract(9, 10, 7, new DateTime(2026, 1, 1), null);
        entity.ContractNumber = "CN-9";
        entity.Comment = "Existing comment";
        entity.Organization = new Organization { ShortName = "Organization" };
        entity.Counterparty = new CounterpartyCard { ShortName = "Supplier", FullName = "Supplier LLC" };
        entity.ContractType = new ContractType { Name = "Supply" };
        entity.State = new State { FullName = "Active" };

        var dto = projection(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.ContractNumber, dto.ContractNumber);
        Assert.Equal(entity.ContractDate, dto.ContractDate);
        Assert.Equal(entity.Comment, dto.Comment);
        Assert.Equal("Supplier LLC", dto.CounterpartyName);
    }

    private static ManualService CreateService(
        IReadOnlyCollection<Contract> contracts,
        IReadOnlyCollection<CounterpartyCard> counterparties)
    {
        var contractRepository = new InMemoryQueryRepository<Contract>(contracts);
        var counterpartyRepository = new InMemoryQueryRepository<CounterpartyCard>(counterparties);
        var userContext = new TestUserContext { OrganizationId = 10 };
        var queryBuilder = new QueryBuilder(null!);
        var constructor = typeof(ManualService).GetConstructors().Single();
        var arguments = constructor.GetParameters()
            .Select(parameter =>
            {
                if (parameter.ParameterType == typeof(IUserContext))
                    return (object)userContext;
                if (parameter.ParameterType == typeof(IQueryBuilder))
                    return queryBuilder;
                if (parameter.ParameterType == typeof(IQueryRepository<Contract>))
                    return contractRepository;
                if (parameter.ParameterType == typeof(IQueryRepository<CounterpartyCard>))
                    return counterpartyRepository;
                return null;
            })
            .ToArray();

        return (ManualService)constructor.Invoke(arguments);
    }

    private static Contract Contract(long id, int organizationId, int counterpartyId, DateTime startDate, DateTime? endDate) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        CounterpartyId = counterpartyId,
        ContractTypeId = 1,
        ContractNumber = $"CN-{id}",
        ContractDate = startDate,
        StartDate = startDate,
        EndDate = endDate,
        StateId = StateIdConst.ACTIVE
    };

    private static CounterpartyCard Counterparty(int id, int organizationId) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        ShortName = $"Counterparty {id}"
    };

    private sealed class TestUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => null;
        public int? TenantId => 1;
        public int? OrganizationId { get; init; }
        public List<int> AllowedOrganizationIds => OrganizationId.HasValue ? [OrganizationId.Value] : [];
        public int? BranchId => null;
    }

    private sealed class InMemoryQueryRepository<TEntity> : IQueryRepository<TEntity>
        where TEntity : class
    {
        private readonly IReadOnlyCollection<TEntity> _items;

        public InMemoryQueryRepository(IReadOnlyCollection<TEntity> items) => _items = items;

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(_items.AsQueryable().Any(predicate));

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_items.AsQueryable().Where(specification.Criteria).ToList());

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            var selector = specification.Selector.Compile();
            var result = _items.AsQueryable()
                .Where(specification.Criteria)
                .Select(selector)
                .Where(specification.ResultCriteria.Compile())
                .ToList();
            return Task.FromResult(result);
        }

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_items.AsQueryable().Where(specification.Criteria).FirstOrDefault());

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(_items.AsQueryable().Where(specification.Criteria).Select(specification.Selector.Compile()).FirstOrDefault());

        public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }
}
