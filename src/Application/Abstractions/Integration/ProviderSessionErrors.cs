using SharedKernel.Results;

namespace Application.Abstractions.Integration;

/// <summary>
/// Error catalogue for the provider-session store. Messages are generic and never contain the
/// scope's TIN/entity id, a token reference or a token fingerprint.
/// </summary>
public static class ProviderSessionErrors
{
    public static readonly Error ScopeInvalid = Error.Forbidden(
        "Integration.SessionScopeInvalid",
        "The organization scope is not valid.");

    public static readonly Error ProviderInvalid = Error.Problem(
        "Integration.ProviderInvalid",
        "The provider is not supported.");

    public static readonly Error TinInvalid = Error.Business(
        "Integration.SessionTinInvalid",
        "The scope TIN is missing or not digits-only.");

    public static readonly Error EntityInvalid = Error.Business(
        "Integration.SessionEntityInvalid",
        "The scope entity id must be non-empty when provided.");

    public static readonly Error CredentialReferenceInvalid = Error.Business(
        "Integration.SessionCredentialReferenceInvalid",
        "A valid provider credential reference is required.");

    public static readonly Error CredentialScopeMismatch = Error.Forbidden(
        "Integration.SessionCredentialScopeMismatch",
        "The provider credential does not match the requested session scope.");

    public static readonly Error AccessReferenceInvalid = Error.Business(
        "Integration.SessionAccessReferenceInvalid",
        "The access token reference is not an accepted secret-manager/envelope reference.");

    public static readonly Error RefreshReferenceInvalid = Error.Business(
        "Integration.SessionRefreshReferenceInvalid",
        "The refresh token reference is not an accepted secret-manager/envelope reference.");

    public static readonly Error FingerprintInvalid = Error.Business(
        "Integration.SessionFingerprintInvalid",
        "The token fingerprint is missing or malformed.");

    public static readonly Error CredentialVersionInvalid = Error.Business(
        "Integration.SessionCredentialVersionInvalid",
        "The credential version must be greater than zero.");

    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "Integration.SessionConcurrencyConflict",
        "The session was modified concurrently. Retry the operation.");
}
