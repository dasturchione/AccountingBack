using SharedKernel.Results;

namespace Application.Abstractions.Integration;

/// <summary>
/// Error catalogue for the provider-credential store. Messages are deliberately generic and
/// never contain the credential scope's TIN, entity id or any secret reference.
/// </summary>
public static class ProviderCredentialErrors
{
    public static readonly Error SecretReferenceInvalid = Error.Business(
        "Integration.CredentialSecretReferenceInvalid",
        "The secret reference is not an accepted secret-manager/envelope reference.");

    public static readonly Error ScopeInvalid = Error.Forbidden(
        "Integration.CredentialScopeInvalid",
        "The organization scope is not valid.");

    public static readonly Error ProviderInvalid = Error.Problem(
        "Integration.ProviderInvalid",
        "The provider is not supported.");

    public static readonly Error KindInvalid = Error.Problem(
        "Integration.CredentialKindInvalid",
        "The credential kind is not supported.");

    public static readonly Error TinInvalid = Error.Business(
        "Integration.CredentialTinInvalid",
        "The scope TIN is missing or not digits-only.");

    public static readonly Error EntityInvalid = Error.Business(
        "Integration.CredentialEntityInvalid",
        "The scope entity id must be non-empty when provided.");

    public static readonly Error KeyVersionInvalid = Error.Business(
        "Integration.CredentialKeyVersionInvalid",
        "The key version must be greater than zero.");

    public static readonly Error ExpiryInvalid = Error.Business(
        "Integration.CredentialExpiryInvalid",
        "The expiry must be later than the validity start.");

    public static readonly Error AlreadyExists = Error.Conflict(
        "Integration.CredentialAlreadyExists",
        "A credential already exists for this scope and kind.");

    public static readonly Error NotFound = Error.NotFound(
        "Integration.CredentialNotFound",
        "No credential exists for this scope and kind.");

    public static readonly Error InvalidTransition = Error.Business(
        "Integration.CredentialInvalidTransition",
        "The credential cannot move to the requested state.");

    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "Integration.CredentialConcurrencyConflict",
        "The credential was modified concurrently. Retry the operation.");
}
