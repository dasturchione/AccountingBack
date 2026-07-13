using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Integration;
using Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using SharedKernel.Results;

namespace Infrastructure.Security;

/// <summary>
/// ASP.NET Core DataProtection implementation of <see cref="ISecretProtector"/>. The organization scope
/// (OrganizationId + Provider + ExternalTin + EntityId) is folded into the DataProtection purpose chain,
/// so a value protected under one scope is cryptographically un-openable under any other scope, and any
/// tampering fails the authenticated-encryption check. The plaintext is never logged, never returned in
/// textual form and never placed in an error. This type performs no network or provider I/O.
/// </summary>
internal sealed class DataProtectionSecretProtector(
    IDataProtectionProvider dataProtectionProvider,
    SecretProtectorOptions options) : ISecretProtector
{
    private const string RootPurpose = "accounting-back.integration.secret.v1";
    private const string SchemePrefix = "envelope://v";

    public Result<EncryptedSecretReference> Protect(OrganizationScope scope, string plaintext)
    {
        if (!IsValidScope(scope))
            return Result.Failure<EncryptedSecretReference>(SecretProtectorErrors.ScopeInvalid);

        if (string.IsNullOrEmpty(plaintext))
            return Result.Failure<EncryptedSecretReference>(SecretProtectorErrors.PlaintextRequired);

        var version = options.CurrentKeyVersion;
        if (version < 1)
            return Result.Failure<EncryptedSecretReference>(SecretProtectorErrors.ProtectFailed);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        try
        {
            if (plaintextBytes.Length > Math.Max(1, options.MaxPlaintextBytes))
                return Result.Failure<EncryptedSecretReference>(SecretProtectorErrors.SecretTooLarge);

            var cipher = CreateProtector(scope).Protect(plaintextBytes);
            var reference = $"{SchemePrefix}{version}/{Base64Url.EncodeToString(cipher)}";

            if (reference.Length > 1000)
                return Result.Failure<EncryptedSecretReference>(SecretProtectorErrors.SecretTooLarge);

            // Guarantees the output is a valid EncryptedSecretReference (credential/session store format).
            var created = EncryptedSecretReference.Create(reference);
            return created.IsSuccess
                ? Result.Success(created.Value)
                : Result.Failure<EncryptedSecretReference>(SecretProtectorErrors.ProtectFailed);
        }
        catch (CryptographicException)
        {
            // No exception detail is surfaced — it could otherwise echo protected material.
            return Result.Failure<EncryptedSecretReference>(SecretProtectorErrors.ProtectFailed);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintextBytes);
        }
    }

    public Result<UnprotectedSecret> Unprotect(OrganizationScope scope, EncryptedSecretReference reference)
    {
        if (!IsValidScope(scope))
            return Result.Failure<UnprotectedSecret>(SecretProtectorErrors.ScopeInvalid);

        if (!TryParse(reference.Value, out var version, out var token))
            return Result.Failure<UnprotectedSecret>(SecretProtectorErrors.ReferenceMalformed);

        if (version < options.MinimumKeyVersion)
            return Result.Failure<UnprotectedSecret>(SecretProtectorErrors.KeyVersionRejected);

        byte[] cipher;
        try
        {
            cipher = Base64Url.DecodeFromChars(token);
        }
        catch (FormatException)
        {
            return Result.Failure<UnprotectedSecret>(SecretProtectorErrors.ReferenceMalformed);
        }

        try
        {
            var plaintextBytes = CreateProtector(scope).Unprotect(cipher);
            return Result.Success(new UnprotectedSecret(plaintextBytes, version));
        }
        catch (CryptographicException)
        {
            // Wrong scope or tampered ciphertext — both fail closed and indistinguishable.
            return Result.Failure<UnprotectedSecret>(SecretProtectorErrors.ScopeMismatchOrTampered);
        }
    }

    private IDataProtector CreateProtector(OrganizationScope scope) =>
        dataProtectionProvider.CreateProtector(
            RootPurpose,
            $"org:{scope.OrganizationId}",
            $"provider:{scope.Provider}",
            $"tin:{scope.ExternalTin.Trim()}",
            $"entity:{scope.EntityId?.Trim() ?? string.Empty}");

    private static bool TryParse(string reference, out int version, out ReadOnlySpan<char> token)
    {
        version = 0;
        token = default;

        if (string.IsNullOrEmpty(reference) || !reference.StartsWith(SchemePrefix, StringComparison.Ordinal))
            return false;

        var rest = reference.AsSpan(SchemePrefix.Length);
        var slash = rest.IndexOf('/');
        if (slash <= 0 || slash == rest.Length - 1)
            return false;

        if (!int.TryParse(rest[..slash], out version) || version < 1)
            return false;

        token = rest[(slash + 1)..];
        return !token.IsEmpty;
    }

    private static bool IsValidScope(OrganizationScope scope)
    {
        if (scope is null || scope.OrganizationId <= 0 || !Enum.IsDefined(scope.Provider))
            return false;

        if (!IsDigitsOnly(scope.ExternalTin))
            return false;

        return scope.EntityId is null || !string.IsNullOrWhiteSpace(scope.EntityId);
    }

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
