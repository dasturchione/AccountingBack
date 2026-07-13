using Domain.Entities;
using SharedKernel.Results;

namespace Application.Abstractions.Integration;

/// <summary>
/// The material needed to persist a provider session. Tokens are represented only as opaque
/// <see cref="EncryptedSecretReference"/> values plus a non-reversible <paramref name="TokenFingerprint"/>;
/// no plaintext token, key or secret is ever carried here.
/// </summary>
public sealed record ProviderSessionMaterial(
    long ProviderCredentialId,
    EncryptedSecretReference AccessTokenReference,
    EncryptedSecretReference? RefreshTokenReference,
    string TokenFingerprint,
    DateTime? AccessExpiresAtUtc,
    DateTime? RefreshExpiresAtUtc,
    ProviderCredential? Credential = null);

public interface IProviderSessionStore
{
    /// <summary>
    /// Returns the usable session for the scope, or <c>null</c>. A session is usable only when it is
    /// Active, not revoked, its access token has not expired, and it is still bound to the current
    /// (Active, same-version) provider credential. Organization isolation is enforced by the scope.
    /// </summary>
    Task<Result<ProviderSession?>> GetActiveAsync(
        OrganizationScope scope,
        CancellationToken ct = default);

    /// <summary>
    /// Creates or replaces the single session for the scope with freshly issued token references.
    /// </summary>
    Task<Result<ProviderSession>> SetAsync(
        OrganizationScope scope,
        ProviderSessionMaterial material,
        int credentialVersion,
        CancellationToken ct = default);

    /// <summary>
    /// Tenant-scoped conditional 401 invalidation: revokes the scope's session only when its stored
    /// fingerprint matches <paramref name="tokenFingerprint"/> (i.e. the token that received the 401
    /// is still the current one). Returns <c>true</c> when a session was invalidated.
    /// </summary>
    Task<Result<bool>> RemoveIfMatchesAsync(
        OrganizationScope scope,
        string tokenFingerprint,
        CancellationToken ct = default);

    /// <summary>
    /// Revokes every Active session bound to the credential whose <c>CredentialVersion</c> differs
    /// from <paramref name="credentialVersion"/> — used on rotation (pass the new version) and on
    /// revoke (pass a version no session holds). Returns the number of sessions invalidated.
    /// </summary>
    Task<Result<int>> InvalidateForCredentialAsync(
        OrganizationScope scope,
        long providerCredentialId,
        int credentialVersion,
        CancellationToken ct = default);
}
