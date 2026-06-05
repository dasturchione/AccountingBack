using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Organizations;

public interface IOrganizationService
{
    Task<Result<PagedResponse<OrganizationListDto>>> GetAllAsync(OrganizationListFilter filter, CancellationToken ct = default);
    Task<Result<OrganizationDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(OrganizationCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, OrganizationUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
