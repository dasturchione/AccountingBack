using System.Linq.Expressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Cmn.Documents;
using Domain.Entities;
using Infrastructure.Query;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;

namespace UnitTests;

public sealed class DocumentRegistryListContractTests
{
    [Fact]
    public void CriteriaBuilder_FiltersByDocumentTypeCode()
    {
        var sale = CreateDocument(1, "sale_doc", new DateTime(2026, 8, 28));
        var purchase = CreateDocument(2, "purchase_doc", new DateTime(2026, 8, 29));
        var criteria = new DocumentRegistryCriteriaBuilder().Build(new DocumentRegistryListFilter
        {
            DocumentTypeCode = "sale_doc"
        });

        var result = new[] { sale, purchase }.AsQueryable().Where(criteria).ToList();

        Assert.Collection(result, document => Assert.Equal(sale.Id, document.Id));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsFilteredDtoListWithoutPagination()
    {
        var documents = new List<DocumentRegistry>
        {
            CreateDocument(1, "sale_doc", new DateTime(2026, 8, 28)),
            CreateDocument(2, "purchase_doc", new DateTime(2026, 8, 30)),
            CreateDocument(3, "sale_doc", new DateTime(2026, 8, 29))
        };
        var service = new DocumentRegistryService(
            new TestUserContext(),
            new QueryBuilder(new DocumentRegistryQueryBuilderResolver()),
            new InMemoryDocumentRegistryRepository(documents));

        var result = await service.GetAllAsync(new DocumentRegistryListFilter
        {
            DocumentTypeCode = "sale_doc"
        });

        Assert.True(result.IsSuccess);
        var list = Assert.IsType<List<DocumentRegistryDto>>(result.Value);
        Assert.Collection(
            list,
            document => Assert.Equal(3, document.Id),
            document => Assert.Equal(1, document.Id));
    }

    private static DocumentRegistry CreateDocument(long id, string documentTypeCode, DateTime docDate)
    {
        var state = new State
        {
            Id = StateIdConst.ACTIVE,
            ShortName = "Active",
            FullName = "Active"
        };
        return new DocumentRegistry
        {
            Id = id,
            OrganizationId = 1,
            DocumentTypeId = (short)id,
            DocumentType = new DocumentType
            {
                Id = (short)id,
                Code = documentTypeCode,
                Name = documentTypeCode,
                StateId = StateIdConst.ACTIVE,
                State = state
            },
            DocumentId = id * 10,
            DocNumber = id.ToString(),
            DocDate = docDate,
            Amount = id * 100m,
            StateId = StateIdConst.ACTIVE,
            State = state,
            CreatedDate = docDate
        };
    }

    private sealed class DocumentRegistryQueryBuilderResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>()
        {
            if (typeof(TEntity) == typeof(DocumentRegistry) && typeof(TOptions) == typeof(DocumentRegistryListFilter))
                return (ICriteriaBuilder<TEntity, TOptions>)(object)new DocumentRegistryCriteriaBuilder();
            if (typeof(TEntity) == typeof(DocumentRegistryDto) && typeof(TOptions) == typeof(DocumentRegistryListFilter))
                return (ICriteriaBuilder<TEntity, TOptions>)(object)new DocumentRegistryDtoCriteriaBuilder();
            return null;
        }

        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>()
        {
            if (typeof(TEntity) == typeof(DocumentRegistry) && typeof(TResult) == typeof(DocumentRegistryDto))
                return (IProjectionBuilder<TEntity, TResult>)(object)new DocumentRegistryDtoProjection(new TestUserContext());
            throw new InvalidOperationException($"Projection {typeof(TEntity).Name} -> {typeof(TResult).Name} is not configured.");
        }

        public IOrderByBuilder<TEntity, TResult>? GetOrderByBuilder<TEntity, TResult>()
        {
            if (typeof(TEntity) == typeof(DocumentRegistry) && typeof(TResult) == typeof(DocumentRegistryDto))
                return (IOrderByBuilder<TEntity, TResult>)(object)new DocumentRegistryOrderByBuilder();
            return null;
        }
    }

    private sealed class InMemoryDocumentRegistryRepository(List<DocumentRegistry> documents)
        : IQueryRepository<DocumentRegistry>
    {
        public Task<bool> AnyAsync(Expression<Func<DocumentRegistry, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(documents.Any(predicate.Compile()));

        public Task<DocumentRegistry?> GetAsync(QuerySpecification<DocumentRegistry> specification, CancellationToken ct = default) =>
            Task.FromResult(documents.FirstOrDefault(specification.Criteria.Compile()));

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<DocumentRegistry, TResult> specification, CancellationToken ct = default)
        {
            var value = Apply(specification).FirstOrDefault();
            return Task.FromResult(value);
        }

        public Task<List<DocumentRegistry>> GetAllAsync(QuerySpecification<DocumentRegistry> specification, CancellationToken ct = default) =>
            Task.FromResult(documents.Where(specification.Criteria.Compile()).ToList());

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<DocumentRegistry, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(Apply(specification).ToList());

        public Task<PagedList<DocumentRegistry>> GetPagedAsync(PagedQuerySpecification<DocumentRegistry> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<DocumentRegistry, TResult> specification, CancellationToken ct = default)
        {
            var query = documents
                .Where(specification.Criteria.Compile())
                .Select(specification.Selector.Compile())
                .Where(specification.ResultCriteria.Compile())
                .AsQueryable();
            if (specification.OrderBy is not null)
                query = specification.OrderBy(query);
            var all = query.ToList();
            var page = specification.Take.HasValue
                ? all.Skip(specification.Skip).Take(specification.Take.Value).ToList()
                : all;
            return Task.FromResult(new PagedList<TResult>(page, all.Count));
        }

        private IQueryable<TResult> Apply<TResult>(QuerySpecification<DocumentRegistry, TResult> specification)
        {
            var query = documents
                .Where(specification.Criteria.Compile())
                .Select(specification.Selector.Compile())
                .Where(specification.ResultCriteria.Compile())
                .AsQueryable();
            return specification.OrderBy is null ? query : specification.OrderBy(query);
        }
    }

    private sealed class TestUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => LanguageIdConst.RU;
        public int? TenantId => 1;
        public int? OrganizationId => 1;
        public List<int> AllowedOrganizationIds => [1];
        public int? BranchId => null;
    }
}
