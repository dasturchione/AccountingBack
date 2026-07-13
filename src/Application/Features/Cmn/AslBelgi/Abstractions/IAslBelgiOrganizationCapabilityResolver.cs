namespace Application.Features.Cmn.AslBelgi.Abstractions;

public enum AslBelgiOrganizationCapability
{
    Unknown = 0,
    Emitter = 1,
    NonEmitter = 2
}

public interface IAslBelgiOrganizationCapabilityResolver
{
    AslBelgiOrganizationCapability Resolve(int organizationId);
}
