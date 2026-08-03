using Application.Abstractions.Integration.Edo;
using SharedKernel.Constants;

namespace Integration.Edo.Mapping;

public static class EdoProviderCodeMapping
{
    public static string ToIntegrationProviderCode(this EdoProviderCode providerCode) =>
        providerCode switch
        {
            EdoProviderCode.DIDOX => IntegrationProviderConst.Didox,
            EdoProviderCode.FAKTURA => IntegrationProviderConst.Faktura,
            EdoProviderCode.EDOCS => IntegrationProviderConst.Edocs,
            _ => throw new ArgumentOutOfRangeException(nameof(providerCode), providerCode, null)
        };
}
