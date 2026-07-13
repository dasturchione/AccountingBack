using Application.Abstractions;
using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Integration;

/// <summary>
/// Persists provider credentials as staged change-tracker mutations. This store never calls
/// <c>SaveChangesAsync</c>: the calling service owns the unit-of-work transaction and commits
/// all staged changes together. Every organization/TIN/entity value comes from the trusted
/// <see cref="OrganizationScope"/> — never from an untrusted caller — and only opaque
/// <see cref="EncryptedSecretReference"/> values are accepted, so no plaintext secret can enter.
/// </summary>
public sealed class ProviderCredentialStore(
    IQueryRepository<ProviderCredential> credentials,
    ITrackingRepository<ProviderCredential> tracking,
    TimeProvider timeProvider) : IProviderCredentialStore
{
    public async Task<Result<ProviderCredential?>> GetAsync(
        OrganizationScope scope,
        CredentialKind kind,
        CancellationToken ct = default)
    {
        scope = CanonicalizeScope(scope, kind);
        if (ValidateScope(scope, kind) is { IsSuccess: false } invalid)
            return Result.Failure<ProviderCredential?>(invalid.Error);

        var existing = await FindAsync(scope, kind, ct);
        if (existing is null || !IsUsable(existing))
            return Result.Success<ProviderCredential?>(null);

        return Result.Success<ProviderCredential?>(existing);
    }

    public async Task<Result<ProviderCredential>> AddAsync(
        OrganizationScope scope,
        CredentialKind kind,
        EncryptedSecretReference secretReference,
        int keyVersion,
        DateTime validFromUtc,
        DateTime? expiresAtUtc,
        int? createdByUserId,
        CancellationToken ct = default)
    {
        scope = CanonicalizeScope(scope, kind);
        if (ValidateMutation(scope, kind, secretReference, keyVersion, validFromUtc, expiresAtUtc)
            is { IsSuccess: false } invalid)
            return Result.Failure<ProviderCredential>(invalid.Error);

        var existing = await FindAsync(scope, kind, ct);
        if (existing is not null)
            return Result.Failure<ProviderCredential>(ProviderCredentialErrors.AlreadyExists);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var entity = new ProviderCredential
        {
            OrganizationId = scope.OrganizationId,
            Provider = scope.Provider,
            ExternalTin = scope.ExternalTin,
            EntityId = scope.EntityId,
            CredentialKind = kind,
            EncryptedSecretReference = secretReference.Value,
            KeyVersion = keyVersion,
            Status = CredentialStatus.Active,
            ValidFromUtc = validFromUtc,
            ExpiresAtUtc = expiresAtUtc,
            RevokedAtUtc = null,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await tracking.AddAsync(entity, ct);
        return Result.Success(entity);
    }

    public async Task<Result<ProviderCredential>> RotateAsync(
        OrganizationScope scope,
        CredentialKind kind,
        EncryptedSecretReference secretReference,
        int keyVersion,
        DateTime validFromUtc,
        DateTime? expiresAtUtc,
        int? createdByUserId,
        CancellationToken ct = default)
    {
        scope = CanonicalizeScope(scope, kind);
        if (ValidateMutation(scope, kind, secretReference, keyVersion, validFromUtc, expiresAtUtc)
            is { IsSuccess: false } invalid)
            return Result.Failure<ProviderCredential>(invalid.Error);

        var existing = await FindAsync(scope, kind, ct);
        if (existing is null)
            return Result.Failure<ProviderCredential>(ProviderCredentialErrors.NotFound);

        if (!CanReactivate(existing.Status))
            return Result.Failure<ProviderCredential>(ProviderCredentialErrors.InvalidTransition);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        existing.EncryptedSecretReference = secretReference.Value;
        existing.KeyVersion = keyVersion;
        existing.Status = CredentialStatus.Active;
        existing.ValidFromUtc = validFromUtc;
        existing.ExpiresAtUtc = expiresAtUtc;
        existing.RevokedAtUtc = null;
        existing.CreatedByUserId = createdByUserId;
        existing.UpdatedAtUtc = now;

        await tracking.UpdateAsync(existing, ct);
        return Result.Success(existing);
    }

    public async Task<Result> RevokeAsync(
        OrganizationScope scope,
        CredentialKind kind,
        CancellationToken ct = default)
    {
        scope = CanonicalizeScope(scope, kind);
        if (ValidateScope(scope, kind) is { IsSuccess: false } invalid)
            return Result.Failure(invalid.Error);

        var existing = await FindAsync(scope, kind, ct);
        if (existing is null)
            return Result.Failure(ProviderCredentialErrors.NotFound);

        if (existing.Status == CredentialStatus.Revoked)
            return Result.Failure(ProviderCredentialErrors.InvalidTransition);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        existing.Status = CredentialStatus.Revoked;
        existing.RevokedAtUtc = now;
        existing.UpdatedAtUtc = now;

        await tracking.UpdateAsync(existing, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyCollection<string>>> GetActiveEntityIdsAsync(
        OrganizationScope scope,
        CredentialKind kind,
        CancellationToken ct = default)
    {
        if (scope is null || scope.OrganizationId <= 0 || !Enum.IsDefined(scope.Provider)
            || !Enum.IsDefined(kind) || !IsDigitsOnly(scope.ExternalTin))
            return Result.Failure<IReadOnlyCollection<string>>(ProviderCredentialErrors.ScopeInvalid);

        var rows = await credentials.GetAllAsync(new QuerySpecification<ProviderCredential>
        {
            Criteria = c => c.OrganizationId == scope.OrganizationId
                && c.Provider == scope.Provider
                && c.ExternalTin == scope.ExternalTin
                && c.CredentialKind == kind
                && c.Status == CredentialStatus.Active
                && c.EntityId != null
        }, ct);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var ids = rows
            .Where(c => c.RevokedAtUtc is null && (c.ExpiresAtUtc is null || c.ExpiresAtUtc > now))
            .Select(c => c.EntityId!)
            .Select(id => scope.Provider == Provider.EDocs
                ? ProviderScopeCanonicalizer.NormalizeEdocsEntityId(id)
                : id.Trim())
            .OfType<string>()
            .Where(id => id.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return Result.Success<IReadOnlyCollection<string>>(ids);
    }

    // Exactly one row can exist per (organization, provider, tin, entity, kind) — enforced by the
    // ux_int_provider_credential_scope unique index (null entity_id collapses to ''). The scope
    // filter plus the ambient organization query filter keep other organizations invisible.
    private Task<ProviderCredential?> FindAsync(
        OrganizationScope scope,
        CredentialKind kind,
        CancellationToken ct) =>
        credentials.GetAsync(new QuerySpecification<ProviderCredential>
        {
            Criteria = c => c.OrganizationId == scope.OrganizationId
                && c.Provider == scope.Provider
                && c.ExternalTin == scope.ExternalTin
                && c.EntityId == scope.EntityId
                && c.CredentialKind == kind
        }, ct);

    private bool IsUsable(ProviderCredential credential)
    {
        if (credential.Status != CredentialStatus.Active || credential.RevokedAtUtc is not null)
            return false;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        return credential.ExpiresAtUtc is null || credential.ExpiresAtUtc.Value > now;
    }

    // Revoked is terminal; anything else may be reactivated by a rotation.
    private static bool CanReactivate(CredentialStatus status) => status != CredentialStatus.Revoked;

    private static Result ValidateMutation(
        OrganizationScope scope,
        CredentialKind kind,
        EncryptedSecretReference secretReference,
        int keyVersion,
        DateTime validFromUtc,
        DateTime? expiresAtUtc)
    {
        if (ValidateScope(scope, kind) is { IsSuccess: false } invalidScope)
            return invalidScope;

        // Fail closed even if a caller bypassed EncryptedSecretReference.Create somehow.
        if (!EncryptedSecretReference.IsWellFormed(secretReference.Value))
            return Result.Failure(ProviderCredentialErrors.SecretReferenceInvalid);

        if (keyVersion <= 0)
            return Result.Failure(ProviderCredentialErrors.KeyVersionInvalid);

        if (expiresAtUtc is not null && expiresAtUtc.Value <= validFromUtc)
            return Result.Failure(ProviderCredentialErrors.ExpiryInvalid);

        return Result.Success();
    }

    private static Result ValidateScope(OrganizationScope scope, CredentialKind kind)
    {
        if (scope is null || scope.OrganizationId <= 0)
            return Result.Failure(ProviderCredentialErrors.ScopeInvalid);

        if (!Enum.IsDefined(scope.Provider))
            return Result.Failure(ProviderCredentialErrors.ProviderInvalid);

        if (!Enum.IsDefined(kind))
            return Result.Failure(ProviderCredentialErrors.KindInvalid);

        if (!IsDigitsOnly(scope.ExternalTin))
            return Result.Failure(ProviderCredentialErrors.TinInvalid);

        if (scope.EntityId is not null && string.IsNullOrWhiteSpace(scope.EntityId))
            return Result.Failure(ProviderCredentialErrors.EntityInvalid);

        if (scope.Provider == Provider.EDocs
            && kind == CredentialKind.EImzoCertificate
            && string.IsNullOrWhiteSpace(scope.EntityId))
            return Result.Failure(ProviderCredentialErrors.EntityInvalid);

        return Result.Success();
    }

    private static OrganizationScope CanonicalizeScope(OrganizationScope scope, CredentialKind kind) =>
        scope.Provider == Provider.EDocs && kind == CredentialKind.EImzoCertificate
            ? scope with { EntityId = ProviderScopeCanonicalizer.NormalizeEdocsEntityId(scope.EntityId) }
            : scope;

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
