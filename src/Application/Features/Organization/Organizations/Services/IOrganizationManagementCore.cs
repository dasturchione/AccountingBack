using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Organizations;

public interface IOrganizationManagementCore
{
    Task<Result<Organization>> GetOrganizationAsync(
        int organizationId,
        OrganizationManagementOptions options,
        CancellationToken ct = default);

    Task<Result> UpdateOrganizationAsync(
        OrganizationManagementUpdateRequest request,
        OrganizationManagementOptions options,
        CancellationToken ct = default);
}
