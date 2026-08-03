namespace SharedKernel.Exceptions;

public sealed class EdoOrganizationScopeRequiredException : InvalidOperationException
{
    public EdoOrganizationScopeRequiredException()
        : base("An active organization scope is required to resolve the active EDO provider.")
    {
    }
}

public sealed class EdoActiveProviderNotConfiguredException : InvalidOperationException
{
    public EdoActiveProviderNotConfiguredException(int organizationId)
        : base($"An active EDO provider is not configured for organization '{organizationId}'.")
    {
        OrganizationId = organizationId;
    }

    public int OrganizationId { get; }
}

public sealed class EdoActiveProviderStorageUnavailableException : InvalidOperationException
{
    public EdoActiveProviderStorageUnavailableException()
        : base("Active EDO provider persistence is not implemented yet.")
    {
    }
}
