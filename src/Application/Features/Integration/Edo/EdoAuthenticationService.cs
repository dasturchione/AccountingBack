using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Abstractions.Integration.Faktura;
using Application.Features;
using Application.Features.AuditLogs;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Integration.Edo;

public sealed class EdoAuthenticationService(
    IUserContext userContext,
    IActiveEdoProviderResolver activeProviderResolver,
    IEdoAuthCredentialValidator credentialValidator,
    IEdoAuthSigningSessionStore signingSessionStore,
    IAuditLogService auditLogService,
    ILogger<EdoAuthenticationService> logger,
    IUnitOfWork unitOfWork) : BaseService(logger, unitOfWork), IEdoAuthenticationService
{
    private static readonly TimeSpan SigningSessionLifetime = TimeSpan.FromMinutes(5);
    private const string AuthSessionTable = "edo_auth_signing_session";

    public async Task<EdoAuthChallengeDto> GetChallengeAsync(
        EdoAuthChallengeRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);

        EnsureCapability(provider, EdoCapabilityKind.AuthChallenge);
        EnsureAuthMode(provider, request.AuthMode);
        await credentialValidator.ValidateAsync(provider.Code, ct);

        var providerChallenge = await provider.GetAuthChallengeAsync(request, ct);
        var challengeId = RequireValue(providerChallenge.ChallengeId, nameof(providerChallenge.ChallengeId));
        var now = DateTimeOffset.UtcNow;
        var expiresAt = EdoAuthSessionExpiry.Calculate(
            now,
            SigningSessionLifetime,
            providerChallenge.ExpiresAt);
        // Provider TTL is measured before this service receives the response;
        // allow a small transport skew for the exact two-minute contract.
        if (expiresAt < now.Add(EdoAuthSessionExpiry.MinimumChallengeLifetime - TimeSpan.FromSeconds(1))
            || expiresAt > now.Add(EdoAuthSessionExpiry.MaximumChallengeLifetime))
            throw new IntegrationHttpException("EDO provider challenge expiry is outside the allowed 2-5 minute window.", 502);
        var signingMode = ResolveSigningMode(provider);
        var certificateSerialNumber = provider.Code == EdoProviderCode.EDOCS
            ? request.CertificateSerialNumber
            : null;

        EdoAuthSigningSession? session = null;
        await ExecuteInTransactionAsync(nameof(GetChallengeAsync), async () =>
        {
            session = await signingSessionStore.CreateAsync(
                organizationId,
                provider.Code,
                challengeId,
                provider.Code == EdoProviderCode.EDOCS ? challengeId : null,
                certificateSerialNumber,
                signingMode,
                expiresAt,
                ct);

            auditLogService.SetNewValues(new
            {
                operation = "AUTH_CHALLENGE_SESSION_CREATED",
                organizationId,
                providerCode = provider.Code.ToString(),
                sessionId = session.SessionId,
                status = "CREATED"
            });
            await auditLogService.CreateAsync(
                AuthSessionTable,
                session.SessionId,
                AuditLogOperationTypeConst.Create);

            return Result.Success();
        }, ct);

        return new EdoAuthChallengeDto
        {
            ChallengeId = challengeId,
            AuthMode = providerChallenge.AuthMode,
            Payload = providerChallenge.Payload,
            PayloadFormat = providerChallenge.PayloadFormat,
            ExpiresAt = session!.ExpiresAt,
            ChallengeExpiresAt = session.ExpiresAt,
            SigningSessionId = session.SessionId
        };
    }

    public async Task<EdoAuthCompleteDto> CompleteAsync(
        EdoAuthCompleteRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);

        EnsureCapability(provider, EdoCapabilityKind.AuthComplete);
        await credentialValidator.ValidateAsync(provider.Code, ct);

        if (provider.Code == EdoProviderCode.FAKTURA)
        {
            var fakturaResult = await provider.CompleteAuthAsync(request, ct);
            if (!fakturaResult.IsAuthenticated)
                throw new IntegrationUnauthorizedException("Faktura authentication was not completed.");

            return WithAuthenticatedExpiry(fakturaResult, provider.Code);
        }

        if (string.IsNullOrWhiteSpace(request.ChallengeId)
            || string.IsNullOrWhiteSpace(request.SigningSessionId))
        {
            throw new EdoAuthSigningSessionException();
        }

        EdoAuthSigningSession? session = null;
        await ExecuteInTransactionAsync(nameof(CompleteAsync), async () =>
        {
            session = await signingSessionStore.ConsumeAsync(
                organizationId,
                provider.Code,
                request.SigningSessionId,
                request.ChallengeId,
                request.CertificateSerialNumber,
                ct);

            auditLogService.SetOldValues(new
            {
                operation = "AUTH_COMPLETE_SESSION_CONSUMED",
                organizationId,
                providerCode = provider.Code.ToString(),
                sessionId = session.SessionId,
                status = "CREATED"
            });
            auditLogService.SetNewValues(new
            {
                operation = "AUTH_COMPLETE_SESSION_CONSUMED",
                organizationId,
                providerCode = provider.Code.ToString(),
                sessionId = session.SessionId,
                status = "CONSUMED"
            });
            await auditLogService.CreateAsync(
                AuthSessionTable,
                session.SessionId,
                AuditLogOperationTypeConst.Update);

            return Result.Success();
        }, ct);

        var providerResult = await provider.CompleteAuthAsync(request, ct);
        if (!providerResult.IsAuthenticated)
            throw new IntegrationUnauthorizedException("EDO authentication was not completed.");

        var authenticatedExpiresAt = RequireAuthenticatedExpiry(providerResult.ExpiresAt, provider.Code);
        return new EdoAuthCompleteDto
        {
            IsAuthenticated = true,
            SessionId = session!.SessionId,
            ExpiresAt = authenticatedExpiresAt,
            AuthenticatedSessionExpiresAt = authenticatedExpiresAt
        };
    }

    public async Task<EdoAuthCompleteDto> CompleteFakturaAsync(
        FakturaAuthCompleteRequestDto request,
        CancellationToken ct = default)
    {
        _ = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);

        if (provider.Code != EdoProviderCode.FAKTURA)
        {
            throw new EdoCapabilityUnavailableException(
                provider.Code.ToString(),
                "FakturaAuthComplete",
                "ACTIVE_PROVIDER_REQUIRED");
        }

        EnsureCapability(provider, EdoCapabilityKind.AuthComplete);
        await credentialValidator.ValidateAsync(EdoProviderCode.FAKTURA, ct);

        var result = await provider.CompleteAuthAsync(
            new EdoAuthCompleteRequestDto
            {
                PreparedPkcs7 = request.PreparedPkcs7
            },
            ct);
        if (!result.IsAuthenticated)
            throw new IntegrationUnauthorizedException("Faktura authentication was not completed.");

        return WithAuthenticatedExpiry(result, provider.Code);
    }

    private static EdoAuthCompleteDto WithAuthenticatedExpiry(
        EdoAuthCompleteDto result,
        EdoProviderCode providerCode)
    {
        var expiresAt = RequireAuthenticatedExpiry(result.ExpiresAt, providerCode);
        return new EdoAuthCompleteDto
        {
            IsAuthenticated = result.IsAuthenticated,
            SessionId = result.SessionId,
            ExpiresAt = expiresAt,
            AuthenticatedSessionExpiresAt = expiresAt
        };
    }

    private static DateTimeOffset RequireAuthenticatedExpiry(
        DateTimeOffset? providerExpiresAt,
        EdoProviderCode providerCode)
    {
        if (providerExpiresAt is null)
            throw new EdoCapabilityUnavailableException(
                providerCode.ToString(),
                "AuthenticatedSessionExpiry",
                EdoCapabilityStatus.UNKNOWN.ToString());

        var now = DateTimeOffset.UtcNow;
        var expiresAt = EdoAuthSessionExpiry.CapAuthenticatedExpiry(now, providerExpiresAt.Value);
        // Provider TTL is measured before the response reaches this service. A
        // small transport/processing skew must not reject an otherwise valid
        // six-hour provider session.
        if (expiresAt < now.Add(EdoAuthSessionExpiry.MinimumAuthenticatedSessionLifetime - TimeSpan.FromMinutes(1)))
            throw new EdoCapabilityUnavailableException(
                providerCode.ToString(),
                "AuthenticatedSessionExpiry",
                "OUTSIDE_ALLOWED_WINDOW");

        return expiresAt;
    }

    private static void EnsureCapability(IEdoProvider provider, EdoCapabilityKind capability)
    {
        var status = provider.Capabilities.Capabilities
            .Single(item => item.Kind == capability)
            .Status;

        if (status != EdoCapabilityStatus.SUPPORTED)
        {
            throw new EdoCapabilityUnavailableException(
                provider.Code.ToString(),
                capability.ToString(),
                status.ToString());
        }
    }

    private static void EnsureAuthMode(IEdoProvider provider, EdoAuthMode? requestedMode)
    {
        var mode = requestedMode ?? EdoAuthMode.EImzo;
        if (!provider.Capabilities.AuthModes.Contains(mode))
        {
            throw new EdoCapabilityUnavailableException(
                provider.Code.ToString(),
                EdoCapabilityKind.AuthChallenge.ToString(),
                EdoCapabilityStatus.NOT_SUPPORTED.ToString());
        }
    }

    private static EdoSigningMode ResolveSigningMode(IEdoProvider provider)
    {
        if (provider.Capabilities.SigningModes.Count != 1)
        {
            throw new EdoCapabilityUnavailableException(
                provider.Code.ToString(),
                EdoCapabilityKind.AuthChallenge.ToString(),
                EdoCapabilityStatus.UNKNOWN.ToString());
        }

        return provider.Capabilities.SigningModes.Single();
    }

    private static string RequireValue(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{name} is required for EDO authentication.")
            : value;

    private int RequireOrganization() => userContext.OrganizationId
        ?? throw new EdoOrganizationScopeRequiredException();
}
