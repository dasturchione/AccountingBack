using SharedKernel.Results;

namespace Application.Features.OrganizationSetup;

public interface IOrganizationSetupService
{
    Task<Result<OrganizationSetupDto>> GetAsync(CancellationToken ct = default);
    Task<Result> UpdateCompanyProfileAsync(OrganizationSetupCompanyProfileDto dto, CancellationToken ct = default);
    Task<Result> UpdateTaxSettingsAsync(OrganizationSetupTaxSettingsDto dto, CancellationToken ct = default);
    Task<Result> UpdateAccountingPolicyAsync(OrganizationSetupAccountingPolicyDto dto, CancellationToken ct = default);
    Task<Result> UpdateDefaultsAsync(OrganizationSetupDefaultsDto dto, CancellationToken ct = default);
    Task<Result> UpdateUsersAsync(OrganizationSetupUsersDto dto, CancellationToken ct = default);
    Task<Result> CompleteAsync(CancellationToken ct = default);
}
