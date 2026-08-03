using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;

namespace Integration.Edo.Providers;

public sealed class OrganizationActiveEdoProviderResolver(
    IUserContext userContext,
    IActiveEdoProviderStore providerStore,
    IEdoProviderRegistry providerRegistry) : IActiveEdoProviderResolver
{
    public async Task<EdoProviderCode> GetActiveProviderCodeAsync(CancellationToken ct = default)
    {
        var organizationId = GetCurrentOrganizationId();
        var providerCode = await providerStore.GetAsync(organizationId, ct);

        return providerCode
            ?? throw new EdoActiveProviderNotConfiguredException(organizationId);
    }

    public async Task<IEdoProvider> GetActiveProviderAsync(CancellationToken ct = default)
    {
        var providerCode = await GetActiveProviderCodeAsync(ct);
        return providerRegistry.Resolve(providerCode);
    }

    public async Task SetActiveProviderAsync(
        EdoProviderCode providerCode,
        CancellationToken ct = default)
    {
        var organizationId = GetCurrentOrganizationId();

        // Noma'lum provider storage'ga yozilishidan oldin rad etiladi.
        providerRegistry.Resolve(providerCode);
        await providerStore.SetAsync(organizationId, providerCode, ct);
    }

    private int GetCurrentOrganizationId() =>
        userContext.OrganizationId
            ?? throw new EdoOrganizationScopeRequiredException();
}
