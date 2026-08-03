using Application.Abstractions.Integration.Edo;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedKernel.Exceptions;

namespace Integration.Edo.Persistence;

public sealed class EdoDocumentStore(AppDbContext context) : IEdoDocumentStore
{
    public Task<EdoDocument?> FindByIdempotencyAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string idempotencyKey,
        CancellationToken ct = default) =>
        context.EdoDocuments.SingleOrDefaultAsync(document =>
            document.OrganizationId == organizationId
            && document.Provider == providerCode.ToString()
            && document.OperationType == operationType
            && document.IdempotencyKey == idempotencyKey, ct);

    public Task<EdoDocument?> GetAsync(
        int organizationId,
        EdoProviderCode providerCode,
        long id,
        CancellationToken ct = default) =>
        context.EdoDocuments.SingleOrDefaultAsync(document =>
            document.Id == id
            && document.OrganizationId == organizationId
            && document.Provider == providerCode.ToString(), ct);

    public Task<EdoDocument?> FindByProviderDocumentIdAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string providerDocumentId,
        CancellationToken ct = default) =>
        context.EdoDocuments.SingleOrDefaultAsync(document =>
            document.OrganizationId == organizationId
            && document.Provider == providerCode.ToString()
            && document.ProviderDocumentId == providerDocumentId, ct);

    public async Task AddAsync(EdoDocument document, CancellationToken ct = default)
    {
        context.EdoDocuments.Add(document);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            context.Entry(document).State = EntityState.Detached;
            throw new UniqueConstraintViolationException(
                "An EDO document with the same organization, provider, operation, and idempotency key already exists.", ex);
        }
    }

    public Task UpdateAsync(EdoDocument document, CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);

    public async Task<IReadOnlyCollection<EdoDocument>> GetReconciliationCandidatesAsync(
        int organizationId,
        EdoProviderCode providerCode,
        EdoDirection? direction = null,
        CancellationToken ct = default)
    {
        var query = context.EdoDocuments
            .AsNoTracking()
            .Where(document =>
                document.OrganizationId == organizationId
                && document.Provider == providerCode.ToString()
                && document.ProviderDocumentId != null
                && (document.Status == nameof(EdoDocumentStatusCode.RECONCILIATION_REQUIRED)
                    || document.Status == nameof(EdoDocumentStatusCode.PENDING)
                    || document.Status == nameof(EdoDocumentStatusCode.SENT)
                    || document.Status == "SIGN_SENT"));

        if (direction is not null)
            query = query.Where(document => document.Direction == direction.Value.ToString());

        return await query
            .OrderBy(document => document.UpdatedAt ?? document.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<string?> FindProviderResultIdAsync(
        int organizationId,
        string providerScopedIdempotencyKey,
        CancellationToken ct = default) =>
        context.IdempotencyRecords
            .Where(record => record.OrganizationId == organizationId
                && record.IdempotencyKey == providerScopedIdempotencyKey)
            .Select(record => record.ResultDocumentId)
            .SingleOrDefaultAsync(ct);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
}
