using Application.Abstractions.Authentication;
using Application.Abstractions.Integration;
using Application.Abstractions.Integration.Edo;
using Infrastructure.Persistence;
using Integration.Didox.Configs;
using Integration.Edocs.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Exceptions;

namespace Integration.Edo.Auth;

public sealed class EdoAuthCredentialValidator(
    IUserContext userContext,
    IIntegrationCredentialProvider credentialProvider,
    AppDbContext context,
    IOptions<DidoxOptions> didoxOptions,
    IOptions<EdocsOptions> edocsOptions) : IEdoAuthCredentialValidator
{
    public async Task ValidateAsync(EdoProviderCode providerCode, CancellationToken ct = default)
    {
        var organizationId = userContext.OrganizationId
            ?? throw new EdoOrganizationScopeRequiredException();

        var provider = providerCode switch
        {
            EdoProviderCode.DIDOX => IntegrationProviderConst.Didox,
            EdoProviderCode.EDOCS => IntegrationProviderConst.Edocs,
            EdoProviderCode.FAKTURA => IntegrationProviderConst.Faktura,
            _ => throw new EdoProviderNotFoundException(providerCode.ToString())
        };

        if (providerCode == EdoProviderCode.FAKTURA)
        {
            throw new EdoCapabilityUnavailableException(
                providerCode.ToString(),
                nameof(EdoCapabilityKind.AuthChallenge),
                nameof(EdoCapabilityStatus.UNKNOWN));
        }

        var credential = await credentialProvider.GetAsync(organizationId, provider, ct);
        if (credential is null)
            throw new EdoCredentialNotConfiguredException(providerCode.ToString());

        if (providerCode == EdoProviderCode.DIDOX)
        {
            var organizationInn = await context.Organizations
                .Where(x => x.Id == organizationId)
                .Select(x => x.Inn)
                .SingleOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(organizationInn)
                || string.IsNullOrWhiteSpace(didoxOptions.Value.BaseUrl)
                || string.IsNullOrWhiteSpace(didoxOptions.Value.PartnerToken))
            {
                throw new EdoCredentialNotConfiguredException(providerCode.ToString());
            }
        }

        if (providerCode == EdoProviderCode.EDOCS
            && (string.IsNullOrWhiteSpace(credential.PartnerId)
                || string.IsNullOrWhiteSpace(edocsOptions.Value.BaseUrl)))
        {
            throw new EdoCredentialNotConfiguredException(providerCode.ToString());
        }
    }
}
