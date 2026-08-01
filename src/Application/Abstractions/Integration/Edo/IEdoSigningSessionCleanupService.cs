namespace Application.Abstractions.Integration.Edo;

public interface IEdoSigningSessionCleanupService
{
    Task<EdoSigningSessionCleanupResultDto> CleanupAsync(
        int organizationId,
        EdoProviderCode? providerCode,
        TimeSpan consumedRetention,
        CancellationToken ct = default);
}

public sealed class EdoSigningSessionCleanupResultDto
{
    public int OrganizationId { get; init; }
    public EdoProviderCode? ProviderCode { get; init; }
    public int AuthSessionsRemoved { get; init; }
    public int DocumentSessionsRemoved { get; init; }
    public DateTimeOffset ExecutedAt { get; init; }
}
