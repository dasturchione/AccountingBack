using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Application.Features.Contracts;
using Domain.Entities;
using Infrastructure.Query;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Query.Builders;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

public sealed class ProviderContractReconciliationServiceTests
{
    [Fact]
    public async Task CreatesProviderLinkedContractAndReusesItOnRetry()
    {
        var contracts = new List<Contract>();
        var counterparty = Counterparty(2, 5);
        var service = CreateService(2, counterparty, contracts);
        var dto = ValidDto();

        var first = await service.ReconcileAsync(dto);
        var second = await service.ReconcileAsync(dto);

        Assert.True(first.IsSuccess);
        Assert.Equal("CREATED", first.Value.Status);
        Assert.True(second.IsSuccess);
        Assert.Equal("ALREADY_EXISTS", second.Value.Status);
        Assert.Equal(first.Value.ContractId, second.Value.ContractId);
        Assert.Single(contracts);
        Assert.Equal("EDOCS", contracts[0].ProviderCode);
        Assert.Equal("3", contracts[0].ProviderContractNumber);
    }

    [Fact]
    public async Task RejectsCounterpartyFromAnotherOrganization()
    {
        var service = CreateService(2, Counterparty(3, 5), []);

        var result = await service.ReconcileAsync(ValidDto());

        Assert.False(result.IsSuccess);
        Assert.Equal("EDO_CONTRACT_COUNTERPARTY_NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task RejectsInactiveProviderIdentityInsteadOfCreatingDuplicate()
    {
        var existing = new Contract
        {
            Id = 91,
            OrganizationId = 2,
            CounterpartyId = 5,
            ProviderCode = "EDOCS",
            ProviderContractNumber = "3",
            ProviderContractDate = new DateOnly(2026, 3, 3),
            StateId = StateIdConst.PASSIVE
        };
        var contracts = new List<Contract> { existing };
        var service = CreateService(2, Counterparty(2, 5), contracts);

        var result = await service.ReconcileAsync(ValidDto());

        Assert.False(result.IsSuccess);
        Assert.Equal("EDO_CONTRACT_PROVIDER_IDENTITY_INACTIVE", result.Error.Code);
        Assert.Single(contracts);
    }

    private static ProviderContractReconciliationService CreateService(
        int organizationId,
        CounterpartyCard counterparty,
        List<Contract> contracts)
    {
        var userContext = new FakeUserContext(organizationId);
        var queryBuilder = new QueryBuilder(new NullQueryBuilderResolver());
        return new ProviderContractReconciliationService(
            userContext,
            queryBuilder,
            new FakeQueryRepository<CounterpartyCard>([counterparty]),
            new FakeQueryRepository<Contract>(contracts),
            new FakeCommandRepository<Contract>(contracts),
            new FakeAuditLogService(),
            NullLogger<ProviderContractReconciliationService>.Instance,
            new FakeUnitOfWork());
    }

    private static ProviderContractReconciliationCreateDto ValidDto() => new()
    {
        Confirm = true,
        CounterpartyId = 5,
        ProviderCode = "EDOCS",
        ProviderContractNumber = "3",
        ProviderContractDate = new DateOnly(2026, 3, 3),
        ContractTypeId = 1,
        ContractDate = new DateTime(2026, 3, 3),
        StartDate = new DateTime(2026, 1, 1),
        EndDate = new DateTime(2026, 12, 31)
    };

    private static CounterpartyCard Counterparty(int organizationId, int id) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        StateId = StateIdConst.ACTIVE,
        ShortName = "Counterparty"
    };

    private sealed class FakeUserContext(int organizationId) : IUserContext
    {
        public int? Id => 10;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => 1;
        public int? TenantId => 1;
        public int? OrganizationId => organizationId;
        public List<int> AllowedOrganizationIds => [organizationId];
        public int? BranchId => null;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeAuditLogService : IAuditLogService
    {
        public void SetOldValues(object oldValues) { }
        public void SetNewValues(object newValues) { }
        public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null, int? organizationId = null) => Task.CompletedTask;
        public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult<List<AuditLogDto>>([]);
    }

    private sealed class NullQueryBuilderResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => throw new NotSupportedException();
        public IOrderByBuilder<TEntity, TResult>? GetOrderByBuilder<TEntity, TResult>() => null;
    }

    private sealed class FakeQueryRepository<TEntity>(IReadOnlyCollection<TEntity> items) : IQueryRepository<TEntity>
        where TEntity : class
    {
        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(items.Any(predicate.Compile()));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(items.AsQueryable().Where(specification.Criteria).FirstOrDefault());

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

    private sealed class FakeCommandRepository<TEntity>(ICollection<TEntity> items) : ICommandRepository<TEntity>
        where TEntity : class
    {
        public Task CreateAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity is Contract contract && contract.Id == 0)
                contract.Id = 100;
            items.Add(entity);
            return Task.CompletedTask;
        }

        public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            foreach (var entity in entities)
                items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
        public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    }
}
