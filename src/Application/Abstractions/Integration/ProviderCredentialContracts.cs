using System.Text.RegularExpressions;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Abstractions.Integration;

public sealed record OrganizationScope(
    int OrganizationId,
    Provider Provider,
    string ExternalTin,
    string? EntityId);

/// <summary>
/// Opaque, validated reference to a secret-manager/envelope-encrypted value. It is never the
/// secret itself: plaintext tokens, passwords, API keys and private-key material (PEM/PFX/P12)
/// are rejected at construction time. Construct only through <see cref="Create"/> so that an
/// invalid value can never be represented. The raw value is never written to logs or exceptions.
/// </summary>
public readonly record struct EncryptedSecretReference
{
    private EncryptedSecretReference(string value) => Value = value;

    /// <summary>The opaque reference string (e.g. <c>kv://vault/name#v3</c>). Not a secret.</summary>
    public string Value { get; }

    public static Result<EncryptedSecretReference> Create(string? value) =>
        IsWellFormed(value)
            ? Result.Success(new EncryptedSecretReference(value!))
            : Result.Failure<EncryptedSecretReference>(ProviderCredentialErrors.SecretReferenceInvalid);

    /// <summary>
    /// True only for an opaque secret-manager/envelope reference. The check is fail-closed:
    /// anything that could carry secret material (whitespace/newlines, PEM/PFX bodies, or a
    /// missing allow-listed scheme) is rejected. The value is never echoed.
    /// </summary>
    public static bool IsWellFormed(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // References are compact single tokens; secret material tends to be long/multi-line.
        if (value.Length is < 8 or > 1000)
            return false;

        // No whitespace or control characters — a reference is a single opaque token.
        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch) || char.IsControl(ch))
                return false;
        }

        // Reject PEM / PKCS / private-key material outright.
        var upper = value.ToUpperInvariant();
        if (upper.Contains("BEGIN") || upper.Contains("END ") ||
            upper.Contains("PRIVATE") || upper.Contains("CERTIFICATE") ||
            value.Contains("-----"))
            return false;

        // Must be an opaque reference URI with an allow-listed scheme.
        return ReferencePattern.IsMatch(value);
    }

    private static readonly Regex ReferencePattern = new(
        @"^(kv|akv|aws-kms|gcp-kms|vault|secretref|secret|envelope)://[A-Za-z0-9._\-/#:?=&%@~]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
}

public interface IOrganizationScopeResolver
{
    Task<Result<OrganizationScope>> ResolveAsync(
        Provider provider,
        string? entityId = null,
        CancellationToken ct = default);
}

public interface IProviderCredentialStore
{
    /// <summary>
    /// Returns the currently usable (Active, not revoked, not expired) credential for the scope
    /// and kind, or <c>null</c> when none is usable. Organization isolation is enforced by the
    /// scope and the ambient organization query filter.
    /// </summary>
    Task<Result<ProviderCredential?>> GetAsync(
        OrganizationScope scope,
        CredentialKind kind,
        CancellationToken ct = default);

    /// <summary>Returns active certificate/entity identities for a trusted organization/TIN scope.</summary>
    Task<Result<IReadOnlyCollection<string>>> GetActiveEntityIdsAsync(
        OrganizationScope scope,
        CredentialKind kind,
        CancellationToken ct = default);

    Task<Result<ProviderCredential>> AddAsync(
        OrganizationScope scope,
        CredentialKind kind,
        EncryptedSecretReference secretReference,
        int keyVersion,
        DateTime validFromUtc,
        DateTime? expiresAtUtc,
        int? createdByUserId,
        CancellationToken ct = default);

    Task<Result<ProviderCredential>> RotateAsync(
        OrganizationScope scope,
        CredentialKind kind,
        EncryptedSecretReference secretReference,
        int keyVersion,
        DateTime validFromUtc,
        DateTime? expiresAtUtc,
        int? createdByUserId,
        CancellationToken ct = default);

    Task<Result> RevokeAsync(
        OrganizationScope scope,
        CredentialKind kind,
        CancellationToken ct = default);
}
