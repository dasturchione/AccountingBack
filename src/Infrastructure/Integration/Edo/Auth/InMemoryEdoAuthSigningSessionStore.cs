using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;
using System.Security.Cryptography;

namespace Integration.Edo.Auth;

/// <summary>
/// Short-lived process-local signing state for the contract-only auth bridge.
/// It is not provider credential storage and must be replaced by a distributed
/// cache when multiple API instances need to share an in-flight signing flow.
/// </summary>
public sealed class InMemoryEdoAuthSigningSessionStore : IEdoAuthSigningSessionStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, EdoAuthSigningSession> _sessions = new(StringComparer.Ordinal);

    public Task<EdoAuthSigningSession> CreateAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string challengeId,
        string? providerChallengeId,
        string? certificateSerialNumber,
        EdoSigningMode signingMode,
        DateTimeOffset expiresAt,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var session = new EdoAuthSigningSession(
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            organizationId,
            providerCode,
            challengeId,
            providerChallengeId,
            certificateSerialNumber,
            signingMode,
            expiresAt);

        lock (_gate)
        {
            RemoveExpired(DateTimeOffset.UtcNow);
            _sessions.Add(session.SessionId, session);
        }

        return Task.FromResult(session);
    }

    public Task<EdoAuthSigningSession> ConsumeAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string sessionId,
        string challengeId,
        string? certificateSerialNumber,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_gate)
        {
            RemoveExpired(DateTimeOffset.UtcNow);

            if (!_sessions.TryGetValue(sessionId, out var session)
                || session.OrganizationId != organizationId
                || session.ProviderCode != providerCode
                || !string.Equals(session.ChallengeId, challengeId, StringComparison.Ordinal)
                || (session.CertificateSerialNumber is not null
                    && !string.Equals(session.CertificateSerialNumber, certificateSerialNumber, StringComparison.Ordinal)))
            {
                throw new EdoAuthSigningSessionException();
            }

            _sessions.Remove(sessionId);
            return Task.FromResult(session);
        }
    }

    public Task<int> CleanupAsync(
        int organizationId,
        EdoProviderCode? providerCode,
        DateTime now,
        DateTime consumedRetentionCutoff,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var removed = 0;
            foreach (var pair in _sessions.ToArray())
            {
                var session = pair.Value;
                if (session.OrganizationId != organizationId
                    || (providerCode is not null && session.ProviderCode != providerCode.Value)
                    || session.ExpiresAt.UtcDateTime > now)
                {
                    continue;
                }

                _sessions.Remove(pair.Key);
                removed++;
            }

            return Task.FromResult(removed);
        }
    }

    private void RemoveExpired(DateTimeOffset now)
    {
        foreach (var pair in _sessions.ToArray())
        {
            if (pair.Value.ExpiresAt <= now)
                _sessions.Remove(pair.Key);
        }
    }
}
