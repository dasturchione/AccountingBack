namespace Application.Abstractions.Integration.Edo;

/// <summary>
/// Provider-neutral state for a short-lived EDO authentication signing flow.
/// The store is deliberately scoped by organization and provider so a session
/// cannot be replayed after changing the active provider or from another org.
/// </summary>
public interface IEdoAuthSigningSessionStore
{
    Task<EdoAuthSigningSession> CreateAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string challengeId,
        string? providerChallengeId,
        string? certificateSerialNumber,
        EdoSigningMode signingMode,
        DateTimeOffset expiresAt,
        CancellationToken ct = default);

    Task<EdoAuthSigningSession> ConsumeAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string sessionId,
        string challengeId,
        string? certificateSerialNumber,
        CancellationToken ct = default);

    Task<int> CleanupAsync(
        int organizationId,
        EdoProviderCode? providerCode,
        DateTime now,
        DateTime consumedRetentionCutoff,
        CancellationToken ct = default);
}

public sealed record EdoAuthSigningSession(
    string SessionId,
    int OrganizationId,
    EdoProviderCode ProviderCode,
    string ChallengeId,
    string? ProviderChallengeId,
    string? CertificateSerialNumber,
    EdoSigningMode SigningMode,
    DateTimeOffset ExpiresAt);
