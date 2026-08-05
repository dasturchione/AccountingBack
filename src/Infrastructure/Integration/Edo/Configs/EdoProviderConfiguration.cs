using Application.Abstractions;
using Application.Abstractions.Integration.Edo;
using Domain.Entities;
using Integration.Didox.Configs;
using Integration.Edocs.Configs;
using Integration.Faktura.Configs;
using Microsoft.Extensions.Options;

namespace Integration.Edo.Configs;

public sealed class EdoProviderConfiguration(
    IQueryRepository<Organization> organizationQuery,
    IOptions<DidoxOptions> didoxOptions,
    IOptions<EdocsOptions> edocsOptions,
    IOptions<FakturaOptions> fakturaOptions) : IEdoProviderConfiguration
{
    public async Task<bool> IsConfiguredAsync(
        EdoProviderCode providerCode,
        int organizationId,
        CancellationToken ct = default) =>
        providerCode switch
        {
            EdoProviderCode.DIDOX => await IsDidoxConfiguredAsync(organizationId, ct),
            EdoProviderCode.EDOCS => IsEdocsConfigured(),
            EdoProviderCode.FAKTURA => IsFakturaConfigured(),
            _ => false
        };

    private async Task<bool> IsDidoxConfiguredAsync(int organizationId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(didoxOptions.Value.BaseUrl))
        {
            return false;
        }

        return await organizationQuery.AnyAsync(
            organization => organization.Id == organizationId
                && !string.IsNullOrWhiteSpace(organization.Inn),
            ct);
    }

    private bool IsEdocsConfigured() =>
        !string.IsNullOrWhiteSpace(edocsOptions.Value.BaseUrl)
        && !string.IsNullOrWhiteSpace(edocsOptions.Value.Product)
        && !string.IsNullOrWhiteSpace(edocsOptions.Value.PartnerId);

    private bool IsFakturaConfigured()
    {
        var options = fakturaOptions.Value;

        return !string.IsNullOrWhiteSpace(options.BaseUrl)
            && !string.IsNullOrWhiteSpace(options.AuthUrl)
            && string.Equals(options.GrantType, "password", StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(options.Username)
            && !string.IsNullOrWhiteSpace(options.Password)
            && !string.IsNullOrWhiteSpace(options.ClientId)
            && !string.IsNullOrWhiteSpace(options.ClientSecret);
    }
}
