using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;

namespace Application.Features.Manual;

public class ManualService : IManualService
{
    private readonly IQueryRepository<Role> _roleQuery;
    private readonly IQueryRepository<State> _stateQuery;
    private readonly IQueryRepository<Region> _regionQuery;
    private readonly IQueryRepository<District> _districtQuery;
    private readonly IQueryRepository<User> _userQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<Unit> _unitQuery;
    private readonly IQueryRepository<DocumentStatus> _documentStatusQuery;
    private readonly IQueryRepository<CounterpartyType> _counterpartyTypeQuery;
    private readonly IQueryRepository<PaymentType> _paymentTypeQuery;

    public ManualService(
        IQueryRepository<Role> roleQuery,
        IQueryRepository<State> stateQuery,
        IQueryRepository<Region> regionQuery,
        IQueryRepository<District> districtQuery,
        IQueryRepository<User> userQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<Unit> unitQuery,
        IQueryRepository<DocumentStatus> documentStatusQuery,
        IQueryRepository<CounterpartyType> counterpartyTypeQuery,
        IQueryRepository<PaymentType> paymentTypeQuery)
    {
        _roleQuery = roleQuery;
        _stateQuery = stateQuery;
        _regionQuery = regionQuery;
        _districtQuery = districtQuery;
        _userQuery = userQuery;
        _currencyQuery = currencyQuery;
        _unitQuery = unitQuery;
        _documentStatusQuery = documentStatusQuery;
        _counterpartyTypeQuery = counterpartyTypeQuery;
        _paymentTypeQuery = paymentTypeQuery;
    }

    public async Task<List<SelectListDto>> GetStatesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<State, SelectListDto>
        {
            Criteria = s => true,
            OrderBy = q => q.OrderBy(s => s.Name),
            Selector = s => new SelectListDto { Id = s.Id, Name = s.FullName }
        };
        var list = await _stateQuery.GetAllAsync(spec, ct);
        return list.ToList();
    }

    public async Task<List<SelectListDto>> GetRegionsAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Region, SelectListDto>
        {
            Criteria = r => r.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(r => r.Name),
            Selector = r => new SelectListDto { Id = r.Id, Name = r.FullName }
        };
        var list = await _regionQuery.GetAllAsync(spec, ct);
        return list.ToList();
    }

    public async Task<List<SelectListDto>> GetDistrictsAsync(int? regionId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<District, SelectListDto>
        {
            Criteria = d => d.StateId == StateIdConst.ACTIVE &&
                            (regionId == null || d.RegionId == regionId),
            OrderBy = q => q.OrderBy(d => d.Name),
            Selector = d => new SelectListDto { Id = d.Id, Name = d.FullName }
        };
        var list = await _districtQuery.GetAllAsync(spec, ct);
        return list.ToList();
    }

    public async Task<List<SelectListDto>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Currency, SelectListDto>
        {
            Criteria = c => c.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(c => c.Name),
            Selector = c => new SelectListDto { Id = c.Id, Name = c.Name, Code = c.Code }
        };
        var list = await _currencyQuery.GetAllAsync(spec, ct);
        return list.ToList();
    }

    public async Task<List<SelectListDto>> GetUnitsAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Unit, SelectListDto>
        {
            Criteria = u => u.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(u => u.Name),
            Selector = u => new SelectListDto { Id = u.Id, Name = u.Name, Code = u.Code }
        };
        var list = await _unitQuery.GetAllAsync(spec, ct);
        return list.ToList();
    }

    public async Task<List<SelectListDto>> GetDocumentStatusesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<DocumentStatus, SelectListDto>
        {
            Criteria = d => d.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(d => d.Name),
            Selector = d => new SelectListDto { Id = d.Id, Name = d.Name, Code = d.Code }
        };
        var list = await _documentStatusQuery.GetAllAsync(spec, ct);
        return list.ToList();
    }

    public async Task<List<SelectListDto>> GetCounterpartyTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<CounterpartyType, SelectListDto>
        {
            Criteria = c => c.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(c => c.Name),
            Selector = c => new SelectListDto { Id = c.Id, Name = c.Name, Code = c.Code }
        };

        var list = await _counterpartyTypeQuery.GetAllAsync(spec, ct);

        return list.ToList();
    }

    public async Task<List<SelectListDto>> GetPaymentTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<PaymentType, SelectListDto>
        {
            Criteria = p => p.StateId == StateIdConst.ACTIVE,
            OrderBy = c => c.OrderBy(p => p.Name),
            Selector = p => new SelectListDto { Id = p.Id, Name = p.Name, Code = p.Code }
        };
        var list = await _paymentTypeQuery.GetAllAsync(spec, ct);
        return list.ToList();
    }

    public async Task<List<SelectListDto>> GetRolesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Role, SelectListDto>
        {
            Criteria = r => r.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(r => r.Name),
            Selector = r => new SelectListDto { Id = r.Id, Name = r.FullName }
        };
        var list = await _roleQuery.GetAllAsync(spec, ct);
        return list.ToList();
    }

    public async Task<List<SelectListDto>> GetUsersAsync(int? roleId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<User, SelectListDto>
        {
            Criteria = u => u.StateId == StateIdConst.ACTIVE &&
                            (roleId == null || u.RoleId == roleId),
            OrderBy = q => q.OrderBy(u => u.Name),
            Selector = u => new SelectListDto { Id = u.Id, Name = $"{u.FirstName} {u.LastName}" }
        };

        var list = await _userQuery.GetAllAsync(spec, ct);

        return list.ToList();
    }
}
