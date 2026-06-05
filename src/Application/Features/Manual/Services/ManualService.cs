using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace Application.Features.Manual;

public class ManualService : IManualService
{
    private readonly IQueryRepository<User> _userQuery;
    private readonly IQueryRepository<Role> _roleQuery;
    private readonly IQueryRepository<State> _stateQuery;
    private readonly IQueryRepository<Region> _regionQuery;
    private readonly IQueryRepository<District> _districtQuery;

    public ManualService(
        IQueryRepository<Role> roleQuery,
        IQueryRepository<State> stateQuery,
        IQueryRepository<Region> regionQuery,
        IQueryRepository<District> districtQuery,
        IQueryRepository<User> userQuery)
    {
        _roleQuery = roleQuery;
        _stateQuery = stateQuery;
        _regionQuery = regionQuery;
        _districtQuery = districtQuery;
        _userQuery = userQuery;
    }

    public async Task<List<SelectListDto>> GetRegionAsync(CancellationToken ct = default)
    {
        var spec = Query.Where<Region>(r => r.StateId == StateIdConst.ACTIVE, q => q.OrderBy(r => r.FullName));

        var list = await _regionQuery.GetAllAsync(spec, ct);

        return list.Select(r => new SelectListDto
        {
            Id = r.Id,
            Name = r.FullName
        }).ToList();
    }

    public async Task<List<SelectListDto>> GetDistrictAsync(int? regionId = null, CancellationToken ct = default)
    {
        var spec = Query.Where<District>(d => d.StateId == StateIdConst.ACTIVE && (regionId == null || d.RegionId == regionId),
                                         q => q.OrderBy(d => d.FullName));

        var list = await _districtQuery.GetAllAsync(spec, ct);

        return list.Select(d => new SelectListDto
        {
            Id = d.Id,
            Name = d.FullName
        }).ToList();
    }

    public async Task<List<SelectListDto>> GetStateAsync(CancellationToken ct = default)
    {
        var spec = Query.Where<State>(s => true, q => q.OrderBy(s => s.FullName));

        var list = await _stateQuery.GetAllAsync(spec, ct);

        return list.Select(s => new SelectListDto
        {
            Id = s.Id,
            Name = s.FullName
        }).ToList();
    }

    public async Task<List<SelectListDto>> GetRolesAsync(CancellationToken ct = default)
    {
        var spec = Query.Where<Role>(r => r.StateId == StateIdConst.ACTIVE, q => q.OrderBy(r => r.FullName));

        var list = await _roleQuery.GetAllAsync(spec, ct);

        return list.Select(r => new SelectListDto
        {
            Id = r.Id,
            Name = r.FullName
        }).ToList();
    }

    public async Task<List<SelectListDto>> GetUsersAsync(int? roleId = null, CancellationToken ct = default)
    {
        var spec = Query.Where<User>(u => u.StateId == StateIdConst.ACTIVE && (roleId == null || u.RoleId == roleId),
                                     q => q.OrderBy(u => u.FirstName).ThenBy(u => u.LastName));

        var list = await _userQuery.GetAllAsync(spec, ct);

        return list.Select(u => new SelectListDto
        {
            Id = u.Id,
            Name = $"{u.FirstName} {u.LastName}"
        }).ToList();
    }
}