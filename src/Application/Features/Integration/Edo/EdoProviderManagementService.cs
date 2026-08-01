using Application.Abstractions.Authentication;
using Application.Abstractions.Integration;
using Application.Abstractions.Integration.Edo;
using SharedKernel.Constants;
using SharedKernel.Exceptions;

namespace Application.Features.Integration.Edo;

public sealed class EdoProviderManagementService(
    IUserContext userContext,
    IEdoProviderRegistry providerRegistry,
    IActiveEdoProviderResolver activeProviderResolver,
    IIntegrationCredentialProvider credentialProvider) : IEdoProviderManagementService
{
    public async Task<IReadOnlyCollection<EdoProviderDto>> GetProvidersAsync(
        CancellationToken ct = default)
    {
        var organizationId = GetCurrentOrganizationId();
        var activeProviderCode = await TryGetActiveProviderCodeAsync(ct);
        var capabilities = providerRegistry.GetProviders();
        var result = new List<EdoProviderDto>(capabilities.Count);

        foreach (var capability in capabilities)
        {
            var isConfigured = await IsConfiguredAsync(
                organizationId,
                capability.ProviderCode,
                ct);

            result.Add(Map(
                capability,
                capability.ProviderCode == activeProviderCode,
                isConfigured));
        }

        return result;
    }

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

    private async Task<EdoProviderCode?> TryGetActiveProviderCodeAsync(
        CancellationToken ct)
    {
        try
        {
            return await activeProviderResolver.GetActiveProviderCodeAsync(ct);
        }
        catch (EdoActiveProviderNotConfiguredException)
        {
            return null;
        }
    }

    private async Task<bool> IsConfiguredAsync(
        int organizationId,
        EdoProviderCode providerCode,
        CancellationToken ct)
    {
        // Faqat organization-scoped IntegrationCredential tekshiriladi.
        // FakturaAuthSettings platforma sozlamasi bu qiymatga ta'sir qilmaydi.
        var credential = await credentialProvider.GetAsync(
            organizationId,
            ToIntegrationProviderCode(providerCode),
            ct);

        return credential is not null;
    }

    private int GetCurrentOrganizationId() =>
        userContext.OrganizationId
            ?? throw new EdoOrganizationScopeRequiredException();

    private static string ToIntegrationProviderCode(EdoProviderCode providerCode) =>
        providerCode switch
        {
            EdoProviderCode.DIDOX => IntegrationProviderConst.Didox,
            EdoProviderCode.FAKTURA => IntegrationProviderConst.Faktura,
            EdoProviderCode.EDOCS => IntegrationProviderConst.Edocs,
            _ => throw new ArgumentOutOfRangeException(nameof(providerCode), providerCode, null)
        };

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
