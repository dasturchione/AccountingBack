using System.Security.Cryptography;
using SharedKernel.Results;

namespace Application.Abstractions.Integration;

/// <summary>
/// Foundation options for <see cref="ISecretProtector"/>. No key material, password or endpoint is
/// configured here — only the application-level key version policy and a size guard.
/// </summary>
public sealed class SecretProtectorOptions
{
    /// <summary>Key version stamped into newly protected references. Must be &gt;= 1.</summary>
    public int CurrentKeyVersion { get; set; } = 1;

    /// <summary>References stamped below this version are rejected fail-closed (forces rotation).</summary>
    public int MinimumKeyVersion { get; set; } = 1;

    /// <summary>Maximum plaintext size in bytes; keeps the produced reference within the 1000-char column.</summary>
    public int MaxPlaintextBytes { get; set; } = 512;
}

/// <summary>
/// The recovered secret bytes plus the key version the reference was stamped with. Holds the plaintext
/// only transiently: <see cref="Dispose"/> zeroes the buffer, and <see cref="ToString"/> never echoes it.
/// </summary>
public sealed class UnprotectedSecret(byte[] value, int keyVersion) : IDisposable
{
    private readonly byte[] _value = value;

    public int KeyVersion { get; } = keyVersion;

    /// <summary>The recovered plaintext bytes. Use immediately and dispose; never log this.</summary>
    public ReadOnlySpan<byte> Value => _value;

    public void Dispose() => CryptographicOperations.ZeroMemory(_value);

    public override string ToString() => "***";
}

/// <summary>
/// Envelope-encrypts secrets bound to an <see cref="OrganizationScope"/> (OrganizationId + Provider +
/// ExternalTin + EntityId). <see cref="Protect"/> returns an opaque <see cref="EncryptedSecretReference"/>;
/// <see cref="Unprotect"/> succeeds only for the exact same scope, and fails closed for a wrong scope,
/// a tampered value, a rejected key version or a malformed reference. The plaintext never appears in a
/// return value's textual form, a log or an error.
/// </summary>
public interface ISecretProtector
{
    Result<EncryptedSecretReference> Protect(OrganizationScope scope, string plaintext);

    Result<UnprotectedSecret> Unprotect(OrganizationScope scope, EncryptedSecretReference reference);
}
