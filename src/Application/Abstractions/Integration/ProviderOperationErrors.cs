using SharedKernel.Results;

namespace Application.Abstractions.Integration;

/// <summary>
/// Error catalogue for the provider-operation idempotency store. Messages are generic and never
/// contain the request payload, request hash or any external identifier.
/// </summary>
public static class ProviderOperationErrors
{
    public static readonly Error ScopeInvalid = Error.Forbidden(
        "Integration.OperationScopeInvalid",
        "The organization scope is not valid.");

    public static readonly Error ProviderInvalid = Error.Problem(
        "Integration.ProviderInvalid",
        "The provider is not supported.");

    public static readonly Error ScopeProviderMismatch = Error.Forbidden(
        "Integration.OperationScopeProviderMismatch",
        "The operation scope does not match the requested provider.");

    public static readonly Error OperationInvalid = Error.Business(
        "Integration.OperationNameInvalid",
        "The operation name is required.");

    public static readonly Error ClientRequestIdInvalid = Error.Business(
        "Integration.OperationClientRequestIdInvalid",
        "The client request id is required.");

    public static readonly Error RequestHashInvalid = Error.Business(
        "Integration.OperationRequestHashInvalid",
        "The request hash is missing or malformed.");

    public static readonly Error HashConflict = Error.Conflict(
        "Integration.OperationHashConflict",
        "The idempotency key was already used with a different request.");

    public static readonly Error NotFound = Error.NotFound(
        "Integration.OperationNotFound",
        "No operation exists for this idempotency key.");

    public static readonly Error InvalidTransition = Error.Business(
        "Integration.OperationInvalidTransition",
        "The operation cannot move to the requested state.");

    public static readonly Error ResendNotAllowed = Error.Business(
        "Integration.OperationResendNotAllowed",
        "The operation can only be resent after it has been marked for reconciliation.");

    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "Integration.OperationConcurrencyConflict",
        "The operation was modified concurrently. Retry the operation.");
}
