using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.DocumentNumbers;
using Application.Features.RetailSaleDocs;
using Application.Features.SaleShipments;
using Domain.Entities;
using Infrastructure.Query;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using System.Linq.Expressions;

namespace UnitTests;

public sealed class DocumentNumberPolicyTests
{
    [Fact]
    public async Task HistoricalDocument_UsesPlainYearlyNumber()
    {
        var service = CreateService();

        var result = await service.GetNextHistoricalAsync(
            organizationId: 7,
            documentTypeId: DocumentTypeIdConst.PURCHASE,
            documentDate: new DateTime(2025, 5, 10));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.SequenceNumber);
        Assert.Equal("1", result.Value.DocumentNumber);
    }

    [Theory]
    [InlineData(DocumentTypeIdConst.PURCHASE)]
    [InlineData(DocumentTypeIdConst.SALE)]
    [InlineData(DocumentTypeIdConst.RETAIL_SALE)]
    public async Task Numbering_StartsFromOneForEveryOrganizationAndYear(short documentTypeId)
    {
        var sequences = new List<DocumentNumberSequence>();
        var service = CreateService(sequences);

        var organization7Year2025 = await service.GetNextAsync(7, documentTypeId, new DateTime(2025, 1, 1));
        var organization7Year2026 = await service.GetNextAsync(7, documentTypeId, new DateTime(2026, 1, 1));
        var organization8Year2026 = await service.GetNextAsync(8, documentTypeId, new DateTime(2026, 1, 1));

        Assert.Equal("1", organization7Year2025.Value.DocumentNumber);
        Assert.Equal("1", organization7Year2026.Value.DocumentNumber);
        Assert.Equal("1", organization8Year2026.Value.DocumentNumber);
    }

    [Fact]
    public void RetailSaleRequests_DoNotAllowManualDocumentNumber()
    {
        Assert.Null(typeof(RetailSaleDocCreateDto).GetProperty("DocNumber"));
        Assert.Null(typeof(RetailSaleDocUpdateDto).GetProperty("DocNumber"));
    }

    [Fact]
    public async Task RetailSale_RejectsDocumentDateEarlierThanLatestNumberedDocument()
    {
        var service = CreateService();

        var latest = await service.GetNextAsync(
            7,
            DocumentTypeIdConst.RETAIL_SALE,
            new DateTime(2026, 8, 27));
        var earlier = await service.GetNextAsync(
            7,
            DocumentTypeIdConst.RETAIL_SALE,
            new DateTime(2026, 8, 26));

        Assert.True(latest.IsSuccess);
        Assert.False(earlier.IsSuccess);
    }

    [Fact]
    public void DocumentCreateAndUpdateRequests_DoNotAllowManualDocumentNumber()
    {
        Assert.Null(typeof(SaleShipmentCreateDto).GetProperty("DocNumber"));
        Assert.Null(typeof(SaleShipmentUpdateDto).GetProperty("DocNumber"));
    }

    [Fact]
    public async Task InventoryAdjustment_NumberRestartsForNewYear_WhenOlderDocumentsExist()
    {
        var service = CreateService(
            inventoryAdjustments:
            [
                new InventoryAdjustmentDoc
                {
                    OrganizationId = 7,
                    DocDate = new DateTime(2025, 12, 31),
                    DocNumber = "9"
                }
            ]);

        var result = await service.GetNextAsync(
            7,
            DocumentTypeIdConst.INVENTORYADJUSTMENT,
            new DateTime(2026, 1, 1));

        Assert.True(result.IsSuccess);
        Assert.Equal("1", result.Value.DocumentNumber);
    }

    private static DocumentNumberService CreateService(
        List<DocumentNumberSequence>? sequences = null,
        List<InventoryAdjustmentDoc>? inventoryAdjustments = null)
    {
        sequences ??= [];
        inventoryAdjustments ??= [];
        return new DocumentNumberService(
            new TestUserContext(),
            new QueryBuilder(new NullQueryBuilderResolver()),
            new NoOpDocumentPostingLock(),
            new InMemoryQueryRepository<Organization>(
            [
                new Organization { Id = 7, TenantId = 1, StateId = StateIdConst.ACTIVE },
                new Organization { Id = 8, TenantId = 1, StateId = StateIdConst.ACTIVE }
            ]),
            new InMemoryQueryRepository<DocumentType>(
            [
                new DocumentType { Id = DocumentTypeIdConst.PURCHASE, Code = "purchase", Name = "Purchase" },
                new DocumentType { Id = DocumentTypeIdConst.SALE, Code = "sale", Name = "Sale" },
                new DocumentType { Id = DocumentTypeIdConst.RETAIL_SALE, Code = "retail_sale", Name = "Retail sale" },
                new DocumentType { Id = DocumentTypeIdConst.INVENTORYADJUSTMENT, Code = "inventory_adjustment", Name = "Inventory adjustment" }
            ]),
            new InMemoryQueryRepository<DocumentNumberSequence>(sequences),
            new InMemoryQueryRepository<InventoryAdjustmentDoc>(inventoryAdjustments),
            new InMemorySequenceCommandRepository(sequences));
    }

    private sealed class InMemoryQueryRepository<TEntity>(List<TEntity> entities)
        : IQueryRepository<TEntity> where TEntity : class
    {
        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(entities.AsQueryable().Any(predicate));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(entities.AsQueryable().Where(specification.Criteria).FirstOrDefault());

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(entities.AsQueryable()
                .Where(specification.Criteria)
                .Select(specification.Selector)
                .Where(specification.ResultCriteria)
                .FirstOrDefault());

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default)
        {
            IQueryable<TEntity> query = entities.AsQueryable().Where(specification.Criteria);
            if (specification.OrderBy is not null)
                query = specification.OrderBy(query);
            return Task.FromResult(query.ToList());
        }

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            IQueryable<TResult> query = entities.AsQueryable()
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

    private sealed class InMemorySequenceCommandRepository(List<DocumentNumberSequence> sequences)
        : ICommandRepository<DocumentNumberSequence>
    {
        public Task CreateAsync(DocumentNumberSequence entity, CancellationToken ct = default)
        {
            sequences.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(DocumentNumberSequence entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task CreateAsync(IEnumerable<DocumentNumberSequence> entities, CancellationToken ct = default) => throw new NotSupportedException();
        public Task UpdateAsync(IEnumerable<DocumentNumberSequence> entities, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeleteAsync(DocumentNumberSequence entity, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeleteAsync(IEnumerable<DocumentNumberSequence> entities, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeleteAsync(Expression<Func<DocumentNumberSequence, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
        public Task ReloadAsync(DocumentNumberSequence entity, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class NoOpDocumentPostingLock : IDocumentPostingLock
    {
        public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
        public Task AcquireInventoryAsync(int organizationId, int warehouseId, IReadOnlyCollection<int> productIds, IReadOnlyCollection<int> productTableIds, CancellationToken ct = default) => Task.CompletedTask;
        public Task AcquireMoneyAsync(int organizationId, string sourceType, int sourceId, CancellationToken ct = default) => Task.CompletedTask;
        public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.FromResult(true);
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
        public int? OrganizationId => 7;
        public List<int> AllowedOrganizationIds => [7, 8];
        public int? BranchId => null;
    }
}
