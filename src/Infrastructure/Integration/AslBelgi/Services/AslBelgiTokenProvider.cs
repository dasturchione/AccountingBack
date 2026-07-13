using System.Security.Cryptography;
using System.Text;
using Application.Abstractions;
using Application.Abstractions.Integration;
using Application.Features.Cmn.AslBelgi.Abstractions;
using Application.Features.Cmn.AslBelgi.Errors;
using Domain.Entities;
using Integration.AslBelgi.Abstractions;
using Integration.AslBelgi.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Results;

namespace Integration.AslBelgi.Services;

/// <summary>
/// Tenant-bound Asl Belgisi credential/session lifecycle. Technical login/password and business
/// api-key values are loaded only from the scoped credential store. Access/refresh tokens are
/// protected and stored in the scoped provider-session store; no process-local token cache exists.
/// </summary>
public sealed class AslBelgiTokenProvider(
    IAslBelgiAuthClient authClient,
    IOrganizationScopeResolver scopeResolver,
    IProviderCredentialStore credentialStore,
    IProviderSessionStore sessionStore,
    ISecretProtector secretProtector,
    IUnitOfWork unitOfWork,
    IOptions<AslBelgiSettings> settings,
    ILogger<AslBelgiTokenProvider> logger) : IAslBelgiTokenProvider
{
    private const string TechnicalMode = "technical";
    private const string BusinessMode = "business";
    private readonly AslBelgiSettings _settings = settings.Value;
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public async Task<Result<string>> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var scope = await ResolveScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<string>(scope.Error);

        if (!IsKnownAuthMode)
            return Result.Failure<string>(AslBelgiErrors.InvalidAuthMode(_settings.AuthMode));

        if (IsBusinessMode)
        {
            var apiKey = await credentialStore.GetAsync(scope.Value, CredentialKind.ApiKey, ct);
            if (!apiKey.IsSuccess || apiKey.Value is null)
                return Result.Failure<string>(CredentialRequired);
            return Unprotect(scope.Value, apiKey.Value.EncryptedSecretReference);
        }

        var credentials = await LoadTechnicalCredentialsAsync(scope.Value, ct);
        if (!credentials.IsSuccess)
            return Result.Failure<string>(credentials.Error);

        var active = await sessionStore.GetActiveAsync(scope.Value, ct);
        if (active.IsSuccess && active.Value is not null)
        {
            var token = Unprotect(scope.Value, active.Value.EncryptedAccessTokenReference);
            if (token.IsSuccess)
                return token;
        }

        await Gate.WaitAsync(ct);
        try
        {
            active = await sessionStore.GetActiveAsync(scope.Value, ct);
            if (active.IsSuccess && active.Value is not null)
            {
                var token = Unprotect(scope.Value, active.Value.EncryptedAccessTokenReference);
                if (token.IsSuccess)
                    return token;
            }

            return await AcquireTechnicalTokenAsync(scope.Value, credentials.Value.Login, credentials.Value.Password, ct);
        }
        finally
        {
            Gate.Release();
        }
    }

    public async Task<Result<string>> RefreshAfterUnauthorizedAsync(string failedAccessToken, CancellationToken ct = default)
    {
        var scope = await ResolveScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<string>(scope.Error);

        if (IsBusinessMode)
            return Result.Failure<string>(CredentialRequired);

        var previousSession = await sessionStore.GetActiveAsync(scope.Value, ct);
        string? previousRefreshToken = null;
        if (previousSession.IsSuccess && previousSession.Value?.EncryptedRefreshTokenReference is { Length: > 0 } previousRefreshReference)
        {
            var previousRefresh = Unprotect(scope.Value, previousRefreshReference);
            if (previousRefresh.IsSuccess)
                previousRefreshToken = previousRefresh.Value;
        }

        var fingerprint = Fingerprint(failedAccessToken);
        var removed = await sessionStore.RemoveIfMatchesAsync(scope.Value, fingerprint, ct);
        if (removed.IsSuccess && removed.Value)
            await SafeSaveAsync(ct);

        var credentials = await LoadTechnicalCredentialsAsync(scope.Value, ct);
        if (!credentials.IsSuccess)
            return Result.Failure<string>(credentials.Error);

        await Gate.WaitAsync(ct);
        try
        {
            var current = await sessionStore.GetActiveAsync(scope.Value, ct);
            if (current.IsSuccess && current.Value is not null)
            {
                var currentToken = Unprotect(scope.Value, current.Value.EncryptedAccessTokenReference);
                if (currentToken.IsSuccess && !string.Equals(currentToken.Value, failedAccessToken, StringComparison.Ordinal))
                    return currentToken;
            }

            if (!string.IsNullOrWhiteSpace(previousRefreshToken))
            {
                try
                {
                    var response = await authClient.RefreshAsync(new AslBelgiRefreshRequest { RefreshToken = previousRefreshToken }, ct);
                    var refreshed = await StoreTokenAsync(scope.Value, credentials.Value.Login, response, ct);
                    if (refreshed.IsSuccess)
                        return refreshed;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning("Asl Belgisi token refresh failed ({ExceptionType}); re-authentication required.", ex.GetType().Name);
                }
            }

            return await AuthenticateAndStoreAsync(scope.Value, credentials.Value.Login, credentials.Value.Password, ct);
        }
        finally
        {
            Gate.Release();
        }
    }

    private async Task<Result<string>> AcquireTechnicalTokenAsync(
        OrganizationScope scope,
        ProviderCredential loginCredential,
        ProviderCredential passwordCredential,
        CancellationToken ct)
    {
        var session = await sessionStore.GetActiveAsync(scope, ct);
        if (session.IsSuccess && session.Value?.EncryptedRefreshTokenReference is { Length: > 0 } refreshReference)
        {
            var refresh = Unprotect(scope, refreshReference);
            if (refresh.IsSuccess)
            {
                try
                {
                    var response = await authClient.RefreshAsync(new AslBelgiRefreshRequest { RefreshToken = refresh.Value }, ct);
                    var refreshed = await StoreTokenAsync(scope, loginCredential, response, ct);
                    if (refreshed.IsSuccess)
                        return refreshed;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning("Asl Belgisi refresh failed ({ExceptionType}); re-authentication required.", ex.GetType().Name);
                }
            }
        }

        return await AuthenticateAndStoreAsync(scope, loginCredential, passwordCredential, ct);
    }

    private async Task<Result<string>> AuthenticateAndStoreAsync(
        OrganizationScope scope,
        ProviderCredential loginCredential,
        ProviderCredential passwordCredential,
        CancellationToken ct)
    {
        var login = Unprotect(scope, loginCredential.EncryptedSecretReference);
        var password = Unprotect(scope, passwordCredential.EncryptedSecretReference);
        if (!login.IsSuccess || !password.IsSuccess)
            return Result.Failure<string>(CredentialRequired);

        try
        {
            var response = await authClient.AuthenticateAsync(
                new AslBelgiAuthRequest { Login = login.Value, Password = password.Value }, ct);
            return await StoreTokenAsync(scope, loginCredential, response, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning("Asl Belgisi authentication failed ({ExceptionType}).", ex.GetType().Name);
            return Result.Failure<string>(SafeTokenFailure);
        }
    }

    private async Task<Result<string>> StoreTokenAsync(
        OrganizationScope scope,
        ProviderCredential credential,
        AslBelgiAuthResponse response,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(response.AccessToken))
            return Result.Failure<string>(SafeTokenFailure);

        var accessReference = secretProtector.Protect(scope, response.AccessToken);
        if (!accessReference.IsSuccess)
            return Result.Failure<string>(SafeTokenFailure);

        EncryptedSecretReference? refreshReference = null;
        if (!string.IsNullOrWhiteSpace(response.RefreshToken))
        {
            var protectedRefresh = secretProtector.Protect(scope, response.RefreshToken);
            if (!protectedRefresh.IsSuccess)
                return Result.Failure<string>(SafeTokenFailure);
            refreshReference = protectedRefresh.Value;
        }

        var lifetimeSeconds = Math.Max(1, response.AccessTokenExpiresIn / 1000 - _settings.AccessTokenSafetyMarginSeconds);
        var saved = await sessionStore.SetAsync(
            scope,
            new ProviderSessionMaterial(
                credential.Id,
                accessReference.Value,
                refreshReference,
                Fingerprint(response.AccessToken),
                DateTime.UtcNow.AddSeconds(lifetimeSeconds),
                string.IsNullOrWhiteSpace(response.RefreshToken) ? null : DateTime.UtcNow.AddHours(24),
                credential),
            credential.KeyVersion,
            ct);
        if (!saved.IsSuccess)
            return Result.Failure<string>(SafeTokenFailure);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<string>(SafeTokenFailure);
        }
        catch (DbUpdateException)
        {
            return Result.Failure<string>(SafeTokenFailure);
        }

        return Result.Success(response.AccessToken);
    }

    private async Task<Result<(ProviderCredential Login, ProviderCredential Password)>> LoadTechnicalCredentialsAsync(
        OrganizationScope scope,
        CancellationToken ct)
    {
        var login = await credentialStore.GetAsync(scope, CredentialKind.TechnicalLogin, ct);
        var password = await credentialStore.GetAsync(scope, CredentialKind.TechnicalPassword, ct);
        if (!login.IsSuccess || !password.IsSuccess || login.Value is null || password.Value is null)
            return Result.Failure<(ProviderCredential, ProviderCredential)>(CredentialRequired);

        return Result.Success((login.Value, password.Value));
    }

    private async Task<Result<OrganizationScope>> ResolveScopeAsync(CancellationToken ct)
    {
        var scope = await scopeResolver.ResolveAsync(Provider.AslBelgi, ct: ct);
        return scope.IsSuccess ? scope : Result.Failure<OrganizationScope>(scope.Error);
    }

    private Result<string> Unprotect(OrganizationScope scope, string referenceValue)
    {
        var reference = EncryptedSecretReference.Create(referenceValue);
        if (!reference.IsSuccess)
            return Result.Failure<string>(CredentialRequired);

        var secret = secretProtector.Unprotect(scope, reference.Value);
        if (!secret.IsSuccess)
            return Result.Failure<string>(CredentialRequired);

        using (secret.Value)
            return Result.Success(Encoding.UTF8.GetString(secret.Value.Value));
    }

    private async Task SafeSaveAsync(CancellationToken ct)
    {
        try { await unitOfWork.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { }
        catch (DbUpdateException) { }
    }

    private bool IsBusinessMode => string.Equals(_settings.AuthMode, BusinessMode, StringComparison.OrdinalIgnoreCase);
    private bool IsKnownAuthMode => IsBusinessMode || string.Equals(_settings.AuthMode, TechnicalMode, StringComparison.OrdinalIgnoreCase);

    private static string Fingerprint(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token ?? string.Empty))).ToLowerInvariant();

    private static readonly Error CredentialRequired = Error.Unauthorized(
        "AslBelgi.CredentialRequired", "Asl Belgisi credential is required for the current organization.");
    private static readonly Error SafeTokenFailure = Error.Problem(
        "AslBelgi.TokenAcquisitionFailed", "Asl Belgisi authentication failed.");
}
