using Application.Features.Cmn.AslBelgi.Abstractions;
using Integration.AslBelgi.Configs;
using Microsoft.Extensions.Options;

namespace Integration.AslBelgi.Services;

public sealed class OptionsAslBelgiOrganizationCapabilityResolver(
    IOptions<AslBelgiSettings> settings) : IAslBelgiOrganizationCapabilityResolver
{
    public AslBelgiOrganizationCapability Resolve(int organizationId)
    {
        if (organizationId <= 0
            || !settings.Value.OrganizationCapabilities.TryGetValue(organizationId, out var configured)
            || !Enum.TryParse<AslBelgiOrganizationCapability>(configured, ignoreCase: true, out var capability)
            || !Enum.IsDefined(capability))
        {
            return AslBelgiOrganizationCapability.Unknown;
        }

        return capability;
    }
}
