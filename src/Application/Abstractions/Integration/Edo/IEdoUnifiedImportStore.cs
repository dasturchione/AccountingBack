using Domain.Entities;

namespace Application.Abstractions.Integration.Edo;

public interface IEdoUnifiedImportStore
{
    Task<EdoImportBatch?> GetBatchAsync(int organizationId, long batchId, CancellationToken ct = default);
    Task<EdoImportBatch?> GetLatestBatchAsync(int organizationId, CancellationToken ct = default);
    Task<EdoImportBatch?> FindByIdempotencyKeyAsync(int organizationId, string key, CancellationToken ct = default);
    Task<IReadOnlyCollection<EdoImportBatchDocument>> GetBatchDocumentsAsync(int organizationId, long batchId, CancellationToken ct = default);
    Task<EdoImportBatchDocument?> FindBatchDocumentAsync(int organizationId, long batchId, string providerDocumentId, CancellationToken ct = default);
    Task AddBatchAsync(EdoImportBatch batch, CancellationToken ct = default);
    Task AddBatchDocumentsAsync(IEnumerable<EdoImportBatchDocument> documents, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task AcquireOrganizationLockAsync(int organizationId, CancellationToken ct = default);
}
