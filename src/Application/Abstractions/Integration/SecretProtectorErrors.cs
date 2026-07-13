using SharedKernel.Results;

namespace Application.Abstractions.Integration;

/// <summary>
/// Error catalogue for <see cref="ISecretProtector"/>. Every message is a fixed, generic string —
/// it never contains the plaintext, the scope TIN/entity or any ciphertext.
/// </summary>
public static class SecretProtectorErrors
{
    public static readonly Error ScopeInvalid = Error.Forbidden(
        "Integration.SecretScopeInvalid",
        "The organization scope is not valid.");

    public static readonly Error PlaintextRequired = Error.Business(
        "Integration.SecretPlaintextRequired",
        "A non-empty secret value is required.");

    public static readonly Error SecretTooLarge = Error.Business(
        "Integration.SecretTooLarge",
        "The secret value exceeds the maximum protected size.");

    public static readonly Error ReferenceMalformed = Error.Business(
        "Integration.SecretReferenceMalformed",
        "The encrypted secret reference is malformed.");

    public static readonly Error KeyVersionRejected = Error.Forbidden(
        "Integration.SecretKeyVersionRejected",
        "The encrypted secret reference uses an unsupported key version.");

    // Wrong scope and tampering are deliberately indistinguishable so a caller cannot probe scopes.
    public static readonly Error ScopeMismatchOrTampered = Error.Forbidden(
        "Integration.SecretScopeMismatchOrTampered",
        "The encrypted secret could not be opened for this scope.");

    public static readonly Error ProtectFailed = Error.Problem(
        "Integration.SecretProtectFailed",
        "The secret could not be protected.");
}
