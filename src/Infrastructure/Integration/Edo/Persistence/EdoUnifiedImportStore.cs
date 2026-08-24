using Application.Abstractions.Integration.Edo;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Integration.Edo.Persistence;

public sealed class EdoUnifiedImportStore(AppDbContext context) : IEdoUnifiedImportStore
{
    public Task<EdoImportBatch?> GetBatchAsync(int organizationId, long batchId, CancellationToken ct = default) =>
        context.EdoImportBatches
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == batchId, ct);

    public Task<EdoImportBatch?> GetLatestBatchAsync(int organizationId, CancellationToken ct = default) =>
        context.EdoImportBatches
            .Include(x => x.Documents)
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

    public Task<EdoImportBatch?> FindByIdempotencyKeyAsync(int organizationId, string key, CancellationToken ct = default) =>
        context.EdoImportBatches
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.IdempotencyKey == key, ct);

    public async Task<IReadOnlyCollection<EdoImportBatchDocument>> GetBatchDocumentsAsync(
        int organizationId,
        long batchId,
        CancellationToken ct = default) =>
        await context.EdoImportBatchDocuments
            .Where(x => x.OrganizationId == organizationId && x.BatchId == batchId)
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

    public Task<EdoImportBatchDocument?> FindBatchDocumentAsync(
        int organizationId,
        long batchId,
        string providerDocumentId,
        CancellationToken ct = default) =>
        context.EdoImportBatchDocuments.FirstOrDefaultAsync(
            x => x.OrganizationId == organizationId &&
                 x.BatchId == batchId &&
                 x.ProviderDocumentId == providerDocumentId,
            ct);

    public Task AddBatchAsync(EdoImportBatch batch, CancellationToken ct = default) =>
        context.EdoImportBatches.AddAsync(batch, ct).AsTask();

    public async Task AddBatchDocumentsAsync(IEnumerable<EdoImportBatchDocument> documents, CancellationToken ct = default) =>
        await context.EdoImportBatchDocuments.AddRangeAsync(documents, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    public async Task AcquireOrganizationLockAsync(int organizationId, CancellationToken ct = default)
    {
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(1782349187, {organizationId})",
            ct);
    }
}
