using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Exceptions;
using System.Security.Cryptography;
using ApplicationSession = Application.Abstractions.Integration.Edo.EdoAuthSigningSession;
using DomainSession = Domain.Entities.EdoAuthSigningSession;

namespace Integration.Edo.Persistence;

/// <summary>
/// Persistent, organization/provider-scoped signing-session store.
/// ConsumedAt is a concurrency token, so only one concurrent consumer can win.
/// </summary>
public sealed class EdoAuthSigningSessionStore(
    AppDbContext context,
    IUserContext userContext) : IEdoAuthSigningSessionStore
{
    public async Task<ApplicationSession> CreateAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string challengeId,
        string? providerChallengeId,
        string? certificateSerialNumber,
        EdoSigningMode signingMode,
        DateTimeOffset expiresAt,
        CancellationToken ct = default)
    {
        EnsureCurrentOrganization(organizationId);
        EnsureKnownProvider(providerCode);

        var entity = new DomainSession
        {
            SessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            OrganizationId = organizationId,
            Provider = providerCode.ToString(),
            ChallengeId = RequireValue(challengeId, nameof(challengeId)),
            ProviderChallengeId = providerChallengeId,
            CertificateSerialNumber = certificateSerialNumber,
            SigningMode = signingMode.ToString(),
            ExpiresAt = expiresAt.UtcDateTime,
            CreatedDate = DateTime.UtcNow
        };

        context.EdoAuthSigningSessions.Add(entity);
        await context.SaveChangesAsync(ct);

        return Map(entity);
    }

    public async Task<ApplicationSession> ConsumeAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string sessionId,
        string challengeId,
        string? certificateSerialNumber,
        CancellationToken ct = default)
    {
        EnsureCurrentOrganization(organizationId);
        EnsureKnownProvider(providerCode);

        var entity = await context.EdoAuthSigningSessions
            .SingleOrDefaultAsync(item =>
                item.SessionId == sessionId
                && item.OrganizationId == organizationId
                && item.Provider == providerCode.ToString()
                && item.ChallengeId == challengeId
                && item.ConsumedAt == null, ct);

        if (entity is null
            || entity.ExpiresAt <= DateTime.UtcNow
            || (entity.CertificateSerialNumber is not null
                && !string.Equals(entity.CertificateSerialNumber, certificateSerialNumber, StringComparison.Ordinal)))
        {
            throw new EdoAuthSigningSessionException();
        }

        entity.ConsumedAt = DateTime.UtcNow;
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new EdoAuthSigningSessionException(ex);
        }

        return Map(entity);
    }

    public async Task<int> CleanupAsync(
        int organizationId,
        EdoProviderCode? providerCode,
        DateTime now,
        DateTime consumedRetentionCutoff,
        CancellationToken ct = default)
    {
        EnsureCurrentOrganization(organizationId);
        if (providerCode is not null)
            EnsureKnownProvider(providerCode.Value);

        var query = context.EdoAuthSigningSessions
            .Where(session => session.OrganizationId == organizationId
                && ((session.ConsumedAt == null && session.ExpiresAt <= now)
                    || (session.ConsumedAt != null && session.ConsumedAt <= consumedRetentionCutoff)));

        if (providerCode is not null)
            query = query.Where(session => session.Provider == providerCode.Value.ToString());

        var sessions = await query.ToListAsync(ct);
        if (sessions.Count == 0)
            return 0;

        context.EdoAuthSigningSessions.RemoveRange(sessions);
        await context.SaveChangesAsync(ct);
        return sessions.Count;
    }

    private void EnsureCurrentOrganization(int organizationId)
    {
        if (userContext.OrganizationId is null)
            throw new EdoOrganizationScopeRequiredException();

        if (userContext.OrganizationId != organizationId)
            throw new InvalidOperationException(
                "The requested organization is outside the current organization scope.");
    }

    private static void EnsureKnownProvider(EdoProviderCode providerCode)
    {
        if (!Enum.IsDefined(providerCode))
            throw new EdoProviderNotFoundException(providerCode.ToString());
    }

    private static string RequireValue(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A non-empty value is required.", name)
            : value;

    private static ApplicationSession Map(DomainSession entity)
    {
        if (!Enum.TryParse<EdoProviderCode>(entity.Provider, ignoreCase: false, out var providerCode)
            || !Enum.IsDefined(providerCode)
            || !Enum.TryParse<EdoSigningMode>(entity.SigningMode, ignoreCase: false, out var signingMode)
            || !Enum.IsDefined(signingMode))
        {
            throw new InvalidOperationException("The stored EDO signing session contains an invalid provider or signing mode.");
        }

        return new ApplicationSession(
            entity.SessionId,
            entity.OrganizationId,
            providerCode,
            entity.ChallengeId,
            entity.ProviderChallengeId,
            entity.CertificateSerialNumber,
            signingMode,
            new DateTimeOffset(DateTime.SpecifyKind(entity.ExpiresAt, DateTimeKind.Utc)));
    }
}
