using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Exceptions;
using System.Security.Cryptography;

namespace Integration.Edo.Persistence;

public sealed class EdoDocumentSigningSessionStore(
    AppDbContext context,
    IUserContext userContext) : IEdoDocumentSigningSessionStore
{
    public async Task<EdoDocumentSigningSession> CreateAsync(
        int organizationId,
        EdoProviderCode providerCode,
        long documentId,
        EdoSigningMode signingMode,
        DateTimeOffset expiresAt,
        CancellationToken ct = default)
    {
        EnsureScope(organizationId);

        var session = new Domain.Entities.EdoDocumentSigningSession
        {
            SessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            OrganizationId = organizationId,
            Provider = providerCode.ToString(),
            DocumentId = documentId,
            SigningMode = signingMode.ToString(),
            ExpiresAt = expiresAt.UtcDateTime,
            CreatedDate = DateTime.UtcNow
        };

        context.EdoDocumentSigningSessions.Add(session);
        await context.SaveChangesAsync(ct);
        return Map(session);
    }

    public async Task<EdoDocumentSigningSession> ConsumeAsync(
        int organizationId,
        EdoProviderCode providerCode,
        long documentId,
        string sessionId,
        CancellationToken ct = default)
    {
        EnsureScope(organizationId);

        var session = await context.EdoDocumentSigningSessions.SingleOrDefaultAsync(item =>
            item.SessionId == sessionId
            && item.OrganizationId == organizationId
            && item.Provider == providerCode.ToString()
            && item.DocumentId == documentId
            && item.ConsumedAt == null, ct);

        if (session is null || session.ExpiresAt <= DateTime.UtcNow)
            throw new EdoAuthSigningSessionException();

        session.ConsumedAt = DateTime.UtcNow;
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new EdoAuthSigningSessionException(ex);
        }

        return Map(session);
    }

    public async Task<int> CleanupAsync(
        int organizationId,
        EdoProviderCode? providerCode,
        DateTime now,
        DateTime consumedRetentionCutoff,
        CancellationToken ct = default)
    {
        EnsureScope(organizationId);
        if (providerCode is not null && !Enum.IsDefined(providerCode.Value))
            throw new EdoProviderNotFoundException(providerCode.Value.ToString());

        var query = context.EdoDocumentSigningSessions
            .Where(session => session.OrganizationId == organizationId
                && ((session.ConsumedAt == null && session.ExpiresAt <= now)
                    || (session.ConsumedAt != null && session.ConsumedAt <= consumedRetentionCutoff)));

        if (providerCode is not null)
            query = query.Where(session => session.Provider == providerCode.Value.ToString());

        var sessions = await query.ToListAsync(ct);
        if (sessions.Count == 0)
            return 0;

        context.EdoDocumentSigningSessions.RemoveRange(sessions);
        await context.SaveChangesAsync(ct);
        return sessions.Count;
    }

    private void EnsureScope(int organizationId)
    {
        if (userContext.OrganizationId is null)
            throw new EdoOrganizationScopeRequiredException();

        if (userContext.OrganizationId != organizationId)
            throw new InvalidOperationException(
                "The requested organization is outside the current organization scope.");
    }

    private static EdoDocumentSigningSession Map(Domain.Entities.EdoDocumentSigningSession entity)
    {
        if (!Enum.TryParse<EdoProviderCode>(entity.Provider, out var providerCode)
            || !Enum.IsDefined(providerCode)
            || !Enum.TryParse<EdoSigningMode>(entity.SigningMode, out var signingMode)
            || !Enum.IsDefined(signingMode))
        {
            throw new InvalidOperationException("The stored EDO document signing session is invalid.");
        }

        return new EdoDocumentSigningSession(
            entity.SessionId,
            entity.OrganizationId,
            providerCode,
            entity.DocumentId,
            signingMode,
            new DateTimeOffset(DateTime.SpecifyKind(entity.ExpiresAt, DateTimeKind.Utc)));
    }
}
