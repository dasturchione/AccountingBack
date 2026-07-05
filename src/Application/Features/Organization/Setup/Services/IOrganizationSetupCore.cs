using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.OrganizationSetup;

public interface IOrganizationSetupCore
{
    Task UpsertTaxSettingsAsync(int organizationId, OrganizationSetupTaxSettingsWriteModel model, CancellationToken ct = default);
    Task UpsertAccountingPolicyAsync(int organizationId, OrganizationSetupAccountingPolicyWriteModel model, CancellationToken ct = default);
    Task UpsertDefaultsAsync(int organizationId, OrganizationSetupDefaultsWriteModel model, CancellationToken ct = default);
    Task SeedWorkspaceSetupAsync(WorkspaceSetupInitializationRequest request, CancellationToken ct = default);
    Task UpdateSetupStateAsync(int organizationId, Action<OrganizationSetupState> update, CancellationToken ct = default);
    Task<Result> CompleteSetupAsync(Organization organization, CancellationToken ct = default);
}
