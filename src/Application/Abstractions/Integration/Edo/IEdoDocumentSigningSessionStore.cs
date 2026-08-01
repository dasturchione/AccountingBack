namespace Application.Abstractions.Integration.Edo;

public interface IEdoDocumentSigningSessionStore
{
    Task<EdoDocumentSigningSession> CreateAsync(
        int organizationId,
        EdoProviderCode providerCode,
        long documentId,
        EdoSigningMode signingMode,
        DateTimeOffset expiresAt,
        CancellationToken ct = default);

    Task<EdoDocumentSigningSession> ConsumeAsync(
        int organizationId,
        EdoProviderCode providerCode,
        long documentId,
        string sessionId,
        CancellationToken ct = default);

    Task<int> CleanupAsync(
        int organizationId,
        EdoProviderCode? providerCode,
        DateTime now,
        DateTime consumedRetentionCutoff,
        CancellationToken ct = default);
}

public sealed record EdoDocumentSigningSession(
    string SessionId,
    int OrganizationId,
    EdoProviderCode ProviderCode,
    long DocumentId,
    EdoSigningMode SigningMode,
    DateTimeOffset ExpiresAt);
