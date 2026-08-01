using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;

namespace Application.Features.Integration.Edo;

public sealed class EdoAuthenticationService(
    IUserContext userContext,
    IActiveEdoProviderResolver activeProviderResolver,
    IEdoAuthCredentialValidator credentialValidator,
    IEdoAuthSigningSessionStore signingSessionStore) : IEdoAuthenticationService
{
    private static readonly TimeSpan SigningSessionLifetime = TimeSpan.FromMinutes(5);

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
        var expiresAt = DateTimeOffset.UtcNow.Add(SigningSessionLifetime);
        var signingMode = ResolveSigningMode(provider);

        var session = await signingSessionStore.CreateAsync(
            organizationId,
            provider.Code,
            challengeId,
            provider.Code == EdoProviderCode.EDOCS ? challengeId : null,
            request.CertificateSerialNumber,
            signingMode,
            expiresAt,
            ct);

        return new EdoAuthChallengeDto
        {
            ChallengeId = challengeId,
            AuthMode = providerChallenge.AuthMode,
            Payload = providerChallenge.Payload,
            PayloadFormat = providerChallenge.PayloadFormat,
            ExpiresAt = session.ExpiresAt,
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

        if (string.IsNullOrWhiteSpace(request.ChallengeId)
            || string.IsNullOrWhiteSpace(request.SigningSessionId))
        {
            throw new EdoAuthSigningSessionException();
        }

        var session = await signingSessionStore.ConsumeAsync(
            organizationId,
            provider.Code,
            request.SigningSessionId,
            request.ChallengeId,
            request.CertificateSerialNumber,
            ct);

        var providerResult = await provider.CompleteAuthAsync(request, ct);
        if (!providerResult.IsAuthenticated)
            throw new IntegrationUnauthorizedException("EDO authentication was not completed.");

        return new EdoAuthCompleteDto
        {
            IsAuthenticated = true,
            SessionId = session.SessionId,
            ExpiresAt = session.ExpiresAt
        };
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
