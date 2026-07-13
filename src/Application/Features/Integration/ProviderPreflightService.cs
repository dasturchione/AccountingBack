using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Integration;

public sealed class ProviderPreflightService(
    IOrganizationScopeResolver scopeResolver,
    IProviderCredentialStore credentialStore,
    IProviderSessionStore sessionStore) : IProviderPreflightService
{
    public async Task<Result<ProviderPreflightResult>> GetAsync(Provider provider, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(provider))
            return Result.Failure<ProviderPreflightResult>(Error.Problem(
                "Integration.ProviderInvalid", "The provider is not supported."));

        var scope = await scopeResolver.ResolveAsync(provider, ct: ct);
        if (!scope.IsSuccess)
            return Result.Failure<ProviderPreflightResult>(scope.Error);

        var contractState = ContractState(provider);
        var credentialPresent = false;
        var sessionPresent = false;
        DateTime? sessionExpiresAtUtc = null;
        var safeErrorCode = (string?)null;
        OrganizationScope? sessionScope = scope.Value;

        if (provider == Provider.EDocs)
        {
            var entities = await credentialStore.GetActiveEntityIdsAsync(
                scope.Value with { EntityId = null }, CredentialKind.EImzoCertificate, ct);
            if (!entities.IsSuccess)
                return Result.Failure<ProviderPreflightResult>(entities.Error);

            if (entities.Value.Count > 1)
            {
                safeErrorCode = "CertificateSelectionRequired";
                sessionScope = null;
            }
            else if (entities.Value.Count == 1)
            {
                sessionScope = scope.Value with { EntityId = entities.Value.Single() };
                var credential = await credentialStore.GetAsync(sessionScope, CredentialKind.EImzoCertificate, ct);
                credentialPresent = credential.IsSuccess && credential.Value is not null;
            }
            else
            {
                safeErrorCode = "CredentialMissing";
                sessionScope = null;
            }
        }
        else if (provider == Provider.Didox)
        {
            var credential = await credentialStore.GetAsync(scope.Value, CredentialKind.CompanyToken, ct);
            credentialPresent = credential.IsSuccess && credential.Value is not null;
        }
        else
        {
            var apiKey = await credentialStore.GetAsync(scope.Value, CredentialKind.ApiKey, ct);
            var login = await credentialStore.GetAsync(scope.Value, CredentialKind.TechnicalLogin, ct);
            var password = await credentialStore.GetAsync(scope.Value, CredentialKind.TechnicalPassword, ct);
            credentialPresent = (apiKey.IsSuccess && apiKey.Value is not null)
                || (login.IsSuccess && login.Value is not null && password.IsSuccess && password.Value is not null);
        }

        if (sessionScope is not null && credentialPresent)
        {
            var session = await sessionStore.GetActiveAsync(sessionScope, ct);
            if (session.IsSuccess && session.Value is not null)
            {
                sessionPresent = true;
                sessionExpiresAtUtc = session.Value.AccessExpiresAtUtc;
            }
        }

        if (contractState == ProviderContractState.Blocked)
            safeErrorCode ??= "ContractBlocked";
        else if (!credentialPresent)
            safeErrorCode ??= "CredentialMissing";
        else if (!sessionPresent)
            safeErrorCode ??= "SessionMissingOrExpired";

        var needsReauthentication = credentialPresent && !sessionPresent;
        var ready = contractState != ProviderContractState.Blocked
            && credentialPresent
            && sessionPresent
            && safeErrorCode is null;

        return Result.Success(new ProviderPreflightResult(
            provider.ToString(),
            ready,
            credentialPresent,
            sessionPresent,
            sessionExpiresAtUtc,
            needsReauthentication,
            contractState.ToString(),
            safeErrorCode));
    }

    private static ProviderContractState ContractState(Provider provider) => provider switch
    {
        Provider.EDocs => ProviderContractState.PartiallyVerified,
        Provider.Didox or Provider.AslBelgi => ProviderContractState.Blocked,
        _ => ProviderContractState.Blocked
    };
}
