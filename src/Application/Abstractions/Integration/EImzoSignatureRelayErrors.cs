using SharedKernel.Results;

namespace Application.Abstractions.Integration;

/// <summary>
/// Error catalogue for the E-IMZO signature relay. Messages are generic and never contain the
/// organization TIN, the challenge content, the payload hash or any signature bytes.
/// </summary>
public static class EImzoSignatureRelayErrors
{
    public static readonly Error UnsupportedContract = Error.Business(
        "Integration.EImzoUnsupportedContract",
        "The provider's E-IMZO canonical payload/mode is not defined for this relay.");

    public static readonly Error ScopeInvalid = Error.Forbidden(
        "Integration.EImzoScopeInvalid",
        "The organization scope is not valid.");

    public static readonly Error SerialRequired = Error.Business(
        "Integration.EImzoSerialRequired",
        "An E-IMZO certificate serial number is required.");

    public static readonly Error ChallengeContentRequired = Error.Business(
        "Integration.EImzoChallengeContentRequired",
        "The challenge content to be signed is required.");

    // One consolidated error for missing / expired / already-consumed challenges — fail-closed and
    // intentionally indistinguishable so a caller cannot probe which challenge ids exist.
    public static readonly Error ChallengeInvalid = Error.Conflict(
        "Integration.EImzoChallengeInvalid",
        "The E-IMZO challenge is expired, already used or does not exist.");

    public static readonly Error ChallengeExpired = Error.Business(
        "Integration.EImzoChallengeExpired",
        "The E-IMZO challenge has expired.");

    public static readonly Error ChallengeMismatch = Error.Forbidden(
        "Integration.EImzoChallengeMismatch",
        "The submission does not match the issued challenge.");

    public static readonly Error OrganizationMismatch = Error.Forbidden(
        "Integration.EImzoOrganizationMismatch",
        "The signature scope does not match the challenge scope.");

    public static readonly Error SerialMismatch = Error.Forbidden(
        "Integration.EImzoSerialMismatch",
        "The certificate serial number does not match the challenge.");

    public static readonly Error PayloadMismatch = Error.Forbidden(
        "Integration.EImzoPayloadMismatch",
        "The signed payload does not match the challenge.");

    public static readonly Error ModeMismatch = Error.Forbidden(
        "Integration.EImzoModeMismatch",
        "The PKCS#7 mode does not match the provider policy.");

    public static readonly Error SignatureMissing = Error.Business(
        "Integration.EImzoSignatureMissing",
        "The PKCS#7 signature is required.");

    public static readonly Error SignatureTooLarge = Error.Business(
        "Integration.EImzoSignatureTooLarge",
        "The PKCS#7 signature exceeds the maximum accepted size.");
}
