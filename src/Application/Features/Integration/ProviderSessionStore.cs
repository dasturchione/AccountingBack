using Application.Abstractions;
using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Integration;

/// <summary>
/// Persists provider sessions as staged change-tracker mutations. Like the credential store it
/// never calls <c>SaveChangesAsync</c>: the calling service owns the unit-of-work transaction.
/// Organization/TIN/entity always come from the trusted <see cref="OrganizationScope"/>, tokens are
/// only ever opaque <see cref="EncryptedSecretReference"/> values, and a session is only handed out
/// while it is still bound to the current Active provider credential version.
/// </summary>
public sealed class ProviderSessionStore(
    IQueryRepository<ProviderSession> sessions,
    ITrackingRepository<ProviderSession> tracking,
    IQueryRepository<ProviderCredential> credentials,
    TimeProvider timeProvider) : IProviderSessionStore
{
    public async Task<Result<ProviderSession?>> GetActiveAsync(
        OrganizationScope scope,
        CancellationToken ct = default)
    {
        scope = CanonicalizeScope(scope);
        if (ValidateScope(scope) is { IsSuccess: false } invalid)
            return Result.Failure<ProviderSession?>(invalid.Error);

        var session = await FindAsync(scope, ct);
        if (session is null || !IsUsable(session))
            return Result.Success<ProviderSession?>(null);

        // Fail closed: the session must still be backed by the current, Active credential version.
        var credential = await credentials.GetAsync(new QuerySpecification<ProviderCredential>
        {
            Criteria = c => c.Id == session.ProviderCredentialId
                && c.OrganizationId == scope.OrganizationId
                && c.Provider == scope.Provider
                && c.ExternalTin == scope.ExternalTin
                && c.EntityId == scope.EntityId
        }, ct);

        if (credential is null
            || credential.Status != CredentialStatus.Active
            || credential.KeyVersion != session.CredentialVersion)
            return Result.Success<ProviderSession?>(null);

        return Result.Success<ProviderSession?>(session);
    }

    public async Task<Result<ProviderSession>> SetAsync(
        OrganizationScope scope,
        ProviderSessionMaterial material,
        int credentialVersion,
        CancellationToken ct = default)
    {
        scope = CanonicalizeScope(scope);
        if (ValidateScope(scope) is { IsSuccess: false } invalidScope)
            return Result.Failure<ProviderSession>(invalidScope.Error);

        if (ValidateMaterial(material, credentialVersion) is { IsSuccess: false } invalidMaterial)
            return Result.Failure<ProviderSession>(invalidMaterial.Error);

        var credential = material.Credential;
        if (credential is null)
        {
            credential = await credentials.GetAsync(new QuerySpecification<ProviderCredential>
            {
                Criteria = c => c.Id == material.ProviderCredentialId
                    && c.OrganizationId == scope.OrganizationId
                    && c.Provider == scope.Provider
                    && c.ExternalTin == scope.ExternalTin
                    && c.EntityId == scope.EntityId
            }, ct);
        }

        if (credential is null || !CredentialMatchesScope(credential, scope))
            return Result.Failure<ProviderSession>(ProviderSessionErrors.CredentialScopeMismatch);

        var credentialId = credential.Id > 0
            ? credential.Id
            : material.ProviderCredentialId;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var existing = await FindAsync(scope, ct);

        if (existing is null)
        {
            var created = new ProviderSession
            {
                ProviderCredentialId = credentialId,
                Credential = credential,
                OrganizationId = scope.OrganizationId,
                Provider = scope.Provider,
                ExternalTin = scope.ExternalTin,
                EntityId = scope.EntityId,
                EncryptedAccessTokenReference = material.AccessTokenReference.Value,
                EncryptedRefreshTokenReference = material.RefreshTokenReference?.Value,
                TokenFingerprint = material.TokenFingerprint,
                AccessExpiresAtUtc = material.AccessExpiresAtUtc,
                RefreshExpiresAtUtc = material.RefreshExpiresAtUtc,
                CredentialVersion = credentialVersion,
                Status = ProviderSessionStatus.Active,
                Last401AtUtc = null,
                RevokedAtUtc = null,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            await tracking.AddAsync(created, ct);
            return Result.Success(created);
        }

        existing.ProviderCredentialId = credentialId;
        existing.Credential = credential;
        existing.EncryptedAccessTokenReference = material.AccessTokenReference.Value;
        existing.EncryptedRefreshTokenReference = material.RefreshTokenReference?.Value;
        existing.TokenFingerprint = material.TokenFingerprint;
        existing.AccessExpiresAtUtc = material.AccessExpiresAtUtc;
        existing.RefreshExpiresAtUtc = material.RefreshExpiresAtUtc;
        existing.CredentialVersion = credentialVersion;
        existing.Status = ProviderSessionStatus.Active;
        existing.Last401AtUtc = null;
        existing.RevokedAtUtc = null;
        existing.UpdatedAtUtc = now;

        await tracking.UpdateAsync(existing, ct);
        return Result.Success(existing);
    }

    public async Task<Result<bool>> RemoveIfMatchesAsync(
        OrganizationScope scope,
        string tokenFingerprint,
        CancellationToken ct = default)
    {
        scope = CanonicalizeScope(scope);
        if (ValidateScope(scope) is { IsSuccess: false } invalid)
            return Result.Failure<bool>(invalid.Error);

        if (string.IsNullOrWhiteSpace(tokenFingerprint))
            return Result.Failure<bool>(ProviderSessionErrors.FingerprintInvalid);

        var session = await FindAsync(scope, ct);

        // Only the session whose current token produced the 401 is invalidated. A mismatch means the
        // session was already rotated to a new token, so it must be left intact.
        if (session is null
            || session.Status != ProviderSessionStatus.Active
            || !FingerprintEquals(session.TokenFingerprint, tokenFingerprint))
            return Result.Success(false);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        session.Status = ProviderSessionStatus.Revoked;
        session.RevokedAtUtc = now;
        session.Last401AtUtc = now;
        session.UpdatedAtUtc = now;

        await tracking.UpdateAsync(session, ct);
        return Result.Success(true);
    }

    public async Task<Result<int>> InvalidateForCredentialAsync(
        OrganizationScope scope,
        long providerCredentialId,
        int credentialVersion,
        CancellationToken ct = default)
    {
        scope = CanonicalizeScope(scope);
        if (ValidateScope(scope) is { IsSuccess: false } invalid)
            return Result.Failure<int>(invalid.Error);

        if (providerCredentialId <= 0)
            return Result.Failure<int>(ProviderSessionErrors.CredentialReferenceInvalid);

        var stale = await sessions.GetAllAsync(new QuerySpecification<ProviderSession>
        {
            Criteria = s => s.ProviderCredentialId == providerCredentialId
                && s.OrganizationId == scope.OrganizationId
                && s.Provider == scope.Provider
                && s.ExternalTin == scope.ExternalTin
                && s.EntityId == scope.EntityId
                && s.Status == ProviderSessionStatus.Active
                && s.CredentialVersion != credentialVersion
        }, ct);

        if (stale.Count == 0)
            return Result.Success(0);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var session in stale)
        {
            session.Status = ProviderSessionStatus.Revoked;
            session.RevokedAtUtc = now;
            session.UpdatedAtUtc = now;
            await tracking.UpdateAsync(session, ct);
        }

        return Result.Success(stale.Count);
    }

    // One row per (organization, provider, tin, entity) — enforced by ux_int_provider_session_scope
    // (null entity_id collapses to ''). The scope filter plus the ambient organization query filter
    // keep other organizations' sessions invisible.
    private Task<ProviderSession?> FindAsync(OrganizationScope scope, CancellationToken ct) =>
        sessions.GetAsync(new QuerySpecification<ProviderSession>
        {
            Criteria = s => s.OrganizationId == scope.OrganizationId
                && s.Provider == scope.Provider
                && s.ExternalTin == scope.ExternalTin
                && s.EntityId == scope.EntityId
        }, ct);

    private bool IsUsable(ProviderSession session)
    {
        if (session.Status != ProviderSessionStatus.Active || session.RevokedAtUtc is not null)
            return false;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        return session.AccessExpiresAtUtc is null || session.AccessExpiresAtUtc.Value > now;
    }

    private static Result ValidateMaterial(ProviderSessionMaterial material, int credentialVersion)
    {
        if (material is null
            || (material.Credential is null && material.ProviderCredentialId <= 0)
            || (material.Credential is not null && material.Credential.Id > 0
                && material.ProviderCredentialId > 0
                && material.Credential.Id != material.ProviderCredentialId))
            return Result.Failure(ProviderSessionErrors.CredentialReferenceInvalid);

        if (!EncryptedSecretReference.IsWellFormed(material.AccessTokenReference.Value))
            return Result.Failure(ProviderSessionErrors.AccessReferenceInvalid);

        if (material.RefreshTokenReference is { } refresh
            && !EncryptedSecretReference.IsWellFormed(refresh.Value))
            return Result.Failure(ProviderSessionErrors.RefreshReferenceInvalid);

        if (!IsValidFingerprint(material.TokenFingerprint))
            return Result.Failure(ProviderSessionErrors.FingerprintInvalid);

        if (credentialVersion <= 0)
            return Result.Failure(ProviderSessionErrors.CredentialVersionInvalid);

        return Result.Success();
    }

    private static Result ValidateScope(OrganizationScope scope)
    {
        if (scope is null || scope.OrganizationId <= 0)
            return Result.Failure(ProviderSessionErrors.ScopeInvalid);

        if (!Enum.IsDefined(scope.Provider))
            return Result.Failure(ProviderSessionErrors.ProviderInvalid);

        if (!IsDigitsOnly(scope.ExternalTin))
            return Result.Failure(ProviderSessionErrors.TinInvalid);

        if (scope.EntityId is not null && string.IsNullOrWhiteSpace(scope.EntityId))
            return Result.Failure(ProviderSessionErrors.EntityInvalid);

        if (scope.Provider == Provider.EDocs && string.IsNullOrWhiteSpace(scope.EntityId))
            return Result.Failure(ProviderSessionErrors.EntityInvalid);

        return Result.Success();
    }

    private static OrganizationScope CanonicalizeScope(OrganizationScope scope) =>
        ProviderScopeCanonicalizer.Canonicalize(scope);

    private static bool CredentialMatchesScope(ProviderCredential credential, OrganizationScope scope) =>
        credential.OrganizationId == scope.OrganizationId
        && credential.Provider == scope.Provider
        && string.Equals(credential.ExternalTin, scope.ExternalTin, StringComparison.Ordinal)
        && string.Equals(credential.EntityId, scope.EntityId, StringComparison.Ordinal);

    // A fingerprint is a compact one-way hash token (hex or base64), never a secret or a reference.
    private static bool IsValidFingerprint(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length is < 16 or > 128)
            return false;

        if (value.Contains("-----") || value.Contains("://"))
            return false;

        foreach (var ch in value)
        {
            var ok = char.IsAsciiLetterOrDigit(ch) || ch is '+' or '/' or '=' or '_' or '-';
            if (!ok)
                return false;
        }

        return true;
    }

    // Length-independent, ordinal comparison — fingerprints are non-secret, but keep it exact.
    private static bool FingerprintEquals(string stored, string candidate) =>
        string.Equals(stored, candidate, StringComparison.Ordinal);

    private static bool IsDigitsOnly(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        foreach (var ch in value)
        {
            if (!char.IsAsciiDigit(ch))
                return false;
        }

        return true;
    }
}
