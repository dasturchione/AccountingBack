using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Manual;
using Domain.Entities;
using Infrastructure.Query;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using System.Linq.Expressions;

namespace UnitTests;

public sealed class BankOperationCategoryManualTests
{
    [Fact]
    public async Task Manual_ReturnsOnlyActiveCategoriesWithLocalizedNamesInNameOrder()
    {
        var repository = new InMemoryQueryRepository<BankOperationCategory>(
            new BankOperationCategory
            {
                Id = 2,
                Code = "BANK_SERVICE",
                Name = "Bank service",
                StateId = StateIdConst.ACTIVE,
                Translations =
                [
                    new BankOperationCategoryTranslation
                    {
                        CategoryId = 2,
                        LanguageId = LanguageIdConst.RU,
                        Name = "Услуга банка"
                    }
                ]
            },
            new BankOperationCategory
            {
                Id = 1,
                Code = "BANK_COMMISSION",
                Name = "Bank commission",
                StateId = StateIdConst.ACTIVE,
                Translations =
                [
                    new BankOperationCategoryTranslation
                    {
                        CategoryId = 1,
                        LanguageId = LanguageIdConst.RU,
                        Name = "Комиссия банка"
                    }
                ]
            },
            new BankOperationCategory
            {
                Id = 3,
                Code = "INACTIVE",
                Name = "Inactive",
                StateId = StateIdConst.PASSIVE
            });
        var service = CreateService(repository);
        var method = typeof(ManualService).GetMethod("GetBankOperationCategoriesAsync");

        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task<List<SelectListDto>>>(
            method.Invoke(service, [CancellationToken.None]));
        var result = await task;

        Assert.Collection(
            result,
            item =>
            {
                Assert.Equal(1, item.Id);
                Assert.Equal("BANK_COMMISSION", item.Code);
                Assert.Equal("Комиссия банка", item.Name);
            },
            item =>
            {
                Assert.Equal(2, item.Id);
                Assert.Equal("BANK_SERVICE", item.Code);
                Assert.Equal("Услуга банка", item.Name);
            });
    }

    private static ManualService CreateService(IQueryRepository<BankOperationCategory> categoryRepository)
    {
        var constructor = Assert.Single(typeof(ManualService).GetConstructors());
        var arguments = constructor.GetParameters()
            .Select<System.Reflection.ParameterInfo, object?>(parameter =>
            {
                if (parameter.ParameterType == typeof(IQueryRepository<BankOperationCategory>))
                    return categoryRepository;
                if (parameter.ParameterType == typeof(IQueryBuilder))
                    return new QueryBuilder(new NullQueryBuilderResolver());
                if (parameter.ParameterType == typeof(IUserContext))
                    return new TestUserContext();
                return null;
            })
            .ToArray();

        return (ManualService)constructor.Invoke(arguments);
    }

    private sealed class InMemoryQueryRepository<TEntity>(params TEntity[] entities)
        : IQueryRepository<TEntity> where TEntity : class
    {
        private readonly List<TEntity> _entities = [.. entities];

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().Any(predicate));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable().Where(specification.Criteria).FirstOrDefault());

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(_entities.AsQueryable()
                .Where(specification.Criteria)
                .Select(specification.Selector)
                .Where(specification.ResultCriteria)
                .FirstOrDefault());

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default)
        {
            IQueryable<TEntity> query = _entities.AsQueryable().Where(specification.Criteria);
            if (specification.OrderBy is not null)
                query = specification.OrderBy(query);
            return Task.FromResult(query.ToList());
        }

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            IQueryable<TResult> query = _entities.AsQueryable()
                .Where(specification.Criteria)
                .Select(specification.Selector)
                .Where(specification.ResultCriteria);
            if (specification.OrderBy is not null)
                query = specification.OrderBy(query);
            return Task.FromResult(query.ToList());
        }

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

    private sealed class TestUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => LanguageIdConst.RU;
        public int? TenantId => 1;
        public int? OrganizationId => 1;
        public List<int> AllowedOrganizationIds => [1];
        public int? BranchId => null;
    }
}
