using Domain.Entities;

namespace Application.Abstractions.Integration.Edo;

public interface IEdoDocumentStore
{
    Task<EdoDocument?> FindByIdempotencyAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string idempotencyKey,
        CancellationToken ct = default);

    Task<EdoDocument?> GetAsync(
        int organizationId,
        EdoProviderCode providerCode,
        long id,
        CancellationToken ct = default);

    Task<EdoDocument?> FindByProviderDocumentIdAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string providerDocumentId,
        CancellationToken ct = default);

    Task AddAsync(EdoDocument document, CancellationToken ct = default);

    Task UpdateAsync(EdoDocument document, CancellationToken ct = default);

    Task<IReadOnlyCollection<EdoDocument>> GetReconciliationCandidatesAsync(
        int organizationId,
        EdoProviderCode providerCode,
        EdoDirection? direction = null,
        CancellationToken ct = default);

    Task<string?> FindProviderResultIdAsync(
        int organizationId,
        string providerScopedIdempotencyKey,
        CancellationToken ct = default);
}
