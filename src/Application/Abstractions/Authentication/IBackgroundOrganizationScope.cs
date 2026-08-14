namespace Application.Abstractions.Authentication;

/// <summary>
/// Provides an explicitly bounded organization scope for trusted background work.
/// The caller must obtain the organization identifier from durable server-side state.
/// </summary>
public interface IBackgroundOrganizationScope
{
    int? OrganizationId { get; }
    bool IsActive { get; }

    IDisposable Enter(int organizationId, string operation);
}
