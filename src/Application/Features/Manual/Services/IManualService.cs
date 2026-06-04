namespace Application.Features.Manual;

public interface IManualService
{
    Task<List<SelectListDto>> GetRegionAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetDistrictAsync(int? regionId = null, CancellationToken ct = default);
    Task<List<SelectListDto>> GetStateAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetRolesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetUsersAsync(int? roleId = null, CancellationToken ct = default);
}
