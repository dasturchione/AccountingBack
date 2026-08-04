using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;

namespace Application.Features.Integration.Edo;

public sealed class EdoProviderManagementService(
    IUserContext userContext,
    IEdoProviderRegistry providerRegistry,
    IActiveEdoProviderResolver activeProviderResolver,
    IEdoProviderConfiguration providerConfiguration) : IEdoProviderManagementService
{
    public async Task<EdoProviderDto> GetActiveProviderAsync(
        CancellationToken ct = default)
    {
        var organizationId = GetCurrentOrganizationId();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        var isConfigured = await IsConfiguredAsync(organizationId, provider.Code, ct);

        return Map(provider.Capabilities, isActive: true, isConfigured);
    }

    public async Task<EdoProviderDto> SetActiveProviderAsync(
        EdoActiveProviderRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = GetCurrentOrganizationId();
        var provider = providerRegistry.Resolve(request.ProviderCode);
        var isConfigured = await IsConfiguredAsync(
            organizationId,
            request.ProviderCode,
            ct);

        if (!isConfigured)
            throw new EdoCredentialNotConfiguredException(request.ProviderCode.ToString());

        await activeProviderResolver.SetActiveProviderAsync(request.ProviderCode, ct);

        return Map(provider.Capabilities, isActive: true, isConfigured: true);
    }

    private async Task<bool> IsConfiguredAsync(
        int organizationId,
        EdoProviderCode providerCode,
        CancellationToken ct) =>
        await providerConfiguration.IsConfiguredAsync(providerCode, organizationId, ct);

    private int GetCurrentOrganizationId() =>
        userContext.OrganizationId
            ?? throw new EdoOrganizationScopeRequiredException();

    private static EdoProviderDto Map(
        EdoProviderCapabilityDto capability,
        bool isActive,
        bool isConfigured) =>
        new()
        {
            ProviderCode = capability.ProviderCode,
            DisplayName = capability.DisplayName,
            IsActive = isActive,
            IsConfigured = isConfigured,
            IsLocalConfigurationOnly = true,
            AuthModes = capability.AuthModes,
            SigningModes = capability.SigningModes,
            Capabilities = capability.Capabilities
        };
}
