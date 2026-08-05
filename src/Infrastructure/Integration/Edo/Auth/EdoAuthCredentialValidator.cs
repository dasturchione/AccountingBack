using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Infrastructure.Persistence;
using Integration.Didox.Configs;
using Integration.Edocs.Configs;
using Integration.Faktura.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel.Exceptions;

namespace Integration.Edo.Auth;

public sealed class EdoAuthCredentialValidator(
    IUserContext userContext,
    AppDbContext context,
    IOptions<DidoxOptions> didoxOptions,
    IOptions<EdocsOptions> edocsOptions,
    IOptions<FakturaOptions> fakturaOptions) : IEdoAuthCredentialValidator
{
    public async Task ValidateAsync(EdoProviderCode providerCode, CancellationToken ct = default)
    {
        var organizationId = userContext.OrganizationId
            ?? throw new EdoOrganizationScopeRequiredException();

        switch (providerCode)
        {
            case EdoProviderCode.DIDOX:
                var organizationInn = await context.Organizations
                    .Where(x => x.Id == organizationId)
                    .Select(x => x.Inn)
                    .SingleOrDefaultAsync(ct);

                if (string.IsNullOrWhiteSpace(organizationInn)
                    || string.IsNullOrWhiteSpace(didoxOptions.Value.BaseUrl))
                {
                    throw new EdoCredentialNotConfiguredException(providerCode.ToString());
                }

                return;

            case EdoProviderCode.EDOCS:
                if (string.IsNullOrWhiteSpace(edocsOptions.Value.BaseUrl)
                    || string.IsNullOrWhiteSpace(edocsOptions.Value.Product)
                    || string.IsNullOrWhiteSpace(edocsOptions.Value.PartnerId))
                {
                    throw new EdoCredentialNotConfiguredException(providerCode.ToString());
                }

                return;

            case EdoProviderCode.FAKTURA:
                if (!IsFakturaConfigured(fakturaOptions.Value))
                    throw new EdoCredentialNotConfiguredException(providerCode.ToString());

                return;

            default:
                throw new EdoProviderNotFoundException(providerCode.ToString());
        }
    }

    private static bool IsFakturaConfigured(FakturaOptions options) =>
        !string.IsNullOrWhiteSpace(options.BaseUrl)
        && !string.IsNullOrWhiteSpace(options.AuthUrl)
        && string.Equals(options.GrantType, "password", StringComparison.Ordinal)
        && !string.IsNullOrWhiteSpace(options.Username)
        && !string.IsNullOrWhiteSpace(options.Password)
        && !string.IsNullOrWhiteSpace(options.ClientId)
        && !string.IsNullOrWhiteSpace(options.ClientSecret);
}
