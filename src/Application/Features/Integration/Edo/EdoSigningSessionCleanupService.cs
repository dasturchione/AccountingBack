using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;

namespace Application.Features.Integration.Edo;

public sealed class EdoSigningSessionCleanupService(
    IUserContext userContext,
    IEdoAuthSigningSessionStore authSessionStore,
    IEdoDocumentSigningSessionStore documentSessionStore) : IEdoSigningSessionCleanupService
{
    public async Task<EdoSigningSessionCleanupResultDto> CleanupAsync(
        int organizationId,
        EdoProviderCode? providerCode,
        TimeSpan consumedRetention,
        CancellationToken ct = default)
    {
        if (userContext.OrganizationId is not int currentOrganizationId)
            throw new EdoOrganizationScopeRequiredException();

        if (currentOrganizationId != organizationId)
            throw new InvalidOperationException(
                "Signing-session cleanup is outside the current organization scope.");

        if (providerCode is not null && !Enum.IsDefined(providerCode.Value))
            throw new EdoProviderNotFoundException(providerCode.Value.ToString());

        if (consumedRetention < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(consumedRetention));

        var now = DateTime.UtcNow;
        var cutoff = now - consumedRetention;
        var authRemoved = await authSessionStore.CleanupAsync(
            organizationId, providerCode, now, cutoff, ct);
        var documentRemoved = await documentSessionStore.CleanupAsync(
            organizationId, providerCode, now, cutoff, ct);

        return new EdoSigningSessionCleanupResultDto
        {
            OrganizationId = organizationId,
            ProviderCode = providerCode,
            AuthSessionsRemoved = authRemoved,
            DocumentSessionsRemoved = documentRemoved,
            ExecutedAt = DateTimeOffset.UtcNow
        };
    }
}
