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
    private readonly IQueryRepository<Bank> _bankQuery;
    private readonly IQueryRepository<DocumentType> _documentTypeQuery;
    private readonly IQueryRepository<OperationType> _operationTypeQuery;
    private readonly IQueryRepository<TaxType> _taxTypeQuery;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly IQueryRepository<Branch> _branchQuery;
    private readonly IQueryRepository<Department> _departmentQuery;
    private readonly IQueryRepository<Position> _positionQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<ProductGroup> _productGroupQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
    private readonly IQueryRepository<OrgBankAccount> _orgBankAccountQuery;
    private readonly IQueryRepository<CashBox> _cashBoxQuery;
    private readonly IQueryRepository<CashOperation> _cashOperationQuery;
    private readonly IQueryRepository<Language> _languageQuery;
    private readonly IQueryRepository<Module>   _moduleQuery;

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
        IQueryRepository<PaymentType> paymentTypeQuery,
        IQueryRepository<Bank> bankQuery,
        IQueryRepository<DocumentType> documentTypeQuery,
        IQueryRepository<OperationType> operationTypeQuery,
        IQueryRepository<TaxType> taxTypeQuery,
        IQueryRepository<VatRate> vatRateQuery,
        IQueryRepository<Branch> branchQuery,
        IQueryRepository<Department> departmentQuery,
        IQueryRepository<Position> positionQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<ProductGroup> productGroupQuery,
        IQueryRepository<Product> productQuery,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<ChartAccount> chartAccountQuery,
        IQueryRepository<OrgBankAccount> orgBankAccountQuery,
        IQueryRepository<CashBox> cashBoxQuery,
        IQueryRepository<CashOperation> cashOperationQuery,
        IQueryRepository<Language> languageQuery,
        IQueryRepository<Module>   moduleQuery)
    {
        _roleQuery             = roleQuery;
        _stateQuery            = stateQuery;
        _regionQuery           = regionQuery;
        _districtQuery         = districtQuery;
        _userQuery             = userQuery;
        _currencyQuery         = currencyQuery;
        _unitQuery             = unitQuery;
        _documentStatusQuery   = documentStatusQuery;
        _counterpartyTypeQuery = counterpartyTypeQuery;
        _paymentTypeQuery      = paymentTypeQuery;
        _bankQuery             = bankQuery;
        _documentTypeQuery     = documentTypeQuery;
        _operationTypeQuery    = operationTypeQuery;
        _taxTypeQuery          = taxTypeQuery;
        _vatRateQuery          = vatRateQuery;
        _branchQuery           = branchQuery;
        _departmentQuery       = departmentQuery;
        _positionQuery         = positionQuery;
        _counterpartyQuery     = counterpartyQuery;
        _productGroupQuery     = productGroupQuery;
        _productQuery          = productQuery;
        _warehouseQuery        = warehouseQuery;
        _chartAccountQuery     = chartAccountQuery;
        _orgBankAccountQuery   = orgBankAccountQuery;
        _cashBoxQuery          = cashBoxQuery;
        _cashOperationQuery    = cashOperationQuery;
        _languageQuery         = languageQuery;
        _moduleQuery           = moduleQuery;
    }

    public async Task<List<SelectListDto>> GetStatesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<State, SelectListDto>
        {
            Criteria = s => true,
            OrderBy  = q => q.OrderBy(s => s.Name),
            Selector = s => new SelectListDto { Id = s.Id, Name = s.FullName }
        };
        return (await _stateQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetRegionsAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Region, SelectListDto>
        {
            Criteria = r => r.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(r => r.Name),
            Selector = r => new SelectListDto { Id = r.Id, Name = r.FullName }
        };
        return (await _regionQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetDistrictsAsync(int? regionId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<District, SelectListDto>
        {
            Criteria = d => d.StateId == StateIdConst.ACTIVE && (regionId == null || d.RegionId == regionId),
            OrderBy  = q => q.OrderBy(d => d.Name),
            Selector = d => new SelectListDto { Id = d.Id, Name = d.FullName }
        };
        return (await _districtQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Currency, SelectListDto>
        {
            Criteria = c => c.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(c => c.Name),
            Selector = c => new SelectListDto { Id = c.Id, Name = c.Name, Code = c.Code }
        };
        return (await _currencyQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetUnitsAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Unit, SelectListDto>
        {
            Criteria = u => u.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(u => u.Name),
            Selector = u => new SelectListDto { Id = u.Id, Name = u.Name, Code = u.Code }
        };
        return (await _unitQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetDocumentStatusesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<DocumentStatus, SelectListDto>
        {
            Criteria = d => d.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(d => d.Name),
            Selector = d => new SelectListDto { Id = d.Id, Name = d.Name, Code = d.Code }
        };
        return (await _documentStatusQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetCounterpartyTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<CounterpartyType, SelectListDto>
        {
            Criteria = c => c.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(c => c.Name),
            Selector = c => new SelectListDto { Id = c.Id, Name = c.Name, Code = c.Code }
        };
        return (await _counterpartyTypeQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetPaymentTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<PaymentType, SelectListDto>
        {
            Criteria = p => p.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(p => p.Name),
            Selector = p => new SelectListDto { Id = p.Id, Name = p.Name, Code = p.Code }
        };
        return (await _paymentTypeQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetRolesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Role, SelectListDto>
        {
            Criteria = r => r.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(r => r.Name),
            Selector = r => new SelectListDto { Id = r.Id, Name = r.FullName }
        };
        return (await _roleQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetUsersAsync(int? roleId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<User, SelectListDto>
        {
            Criteria = u => u.StateId == StateIdConst.ACTIVE && (roleId == null || u.RoleId == roleId),
            OrderBy  = q => q.OrderBy(u => u.Name),
            Selector = u => new SelectListDto { Id = u.Id, Name = $"{u.FirstName} {u.LastName}" }
        };
        return (await _userQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetBanksAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Bank, SelectListDto>
        {
            Criteria = b => b.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(b => b.Name),
            Selector = b => new SelectListDto { Id = b.Id, Name = b.Name, Code = b.Code }
        };
        return (await _bankQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetDocumentTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<DocumentType, SelectListDto>
        {
            Criteria = d => d.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(d => d.Name),
            Selector = d => new SelectListDto { Id = d.Id, Name = d.Name, Code = d.Code }
        };
        return (await _documentTypeQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetOperationTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<OperationType, SelectListDto>
        {
            Criteria = o => o.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(o => o.Name),
            Selector = o => new SelectListDto { Id = o.Id, Name = o.Name, Code = o.Code }
        };
        return (await _operationTypeQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetTaxTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<TaxType, SelectListDto>
        {
            Criteria = t => t.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(t => t.Name),
            Selector = t => new SelectListDto { Id = t.Id, Name = t.Name, Code = t.Code }
        };
        return (await _taxTypeQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetVatRatesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<VatRate, SelectListDto>
        {
            Criteria = v => v.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(v => v.Name),
            Selector = v => new SelectListDto { Id = v.Id, Name = v.Name, Code = v.Code }
        };
        return (await _vatRateQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetBranchesAsync(int? organizationId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Branch, SelectListDto>
        {
            Criteria = b => b.StateId == StateIdConst.ACTIVE && (organizationId == null || b.OrganizationId == organizationId),
            OrderBy  = q => q.OrderBy(b => b.Name),
            Selector = b => new SelectListDto { Id = b.Id, Name = b.Name, Code = b.Code }
        };
        return (await _branchQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetDepartmentsAsync(int? organizationId = null, int? branchId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Department, SelectListDto>
        {
            Criteria = d => d.StateId == StateIdConst.ACTIVE &&
                            (organizationId == null || d.OrganizationId == organizationId) &&
                            (branchId == null || d.BranchId == branchId),
            OrderBy  = q => q.OrderBy(d => d.Name),
            Selector = d => new SelectListDto { Id = d.Id, Name = d.Name, Code = d.Code }
        };
        return (await _departmentQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetPositionsAsync(int? organizationId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Position, SelectListDto>
        {
            Criteria = p => p.StateId == StateIdConst.ACTIVE && (organizationId == null || p.OrganizationId == organizationId),
            OrderBy  = q => q.OrderBy(p => p.Name),
            Selector = p => new SelectListDto { Id = p.Id, Name = p.Name, Code = p.Code }
        };
        return (await _positionQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetCounterpartiesAsync(int? organizationId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<CounterpartyCard, SelectListDto>
        {
            Criteria = c => c.StateId == StateIdConst.ACTIVE && (organizationId == null || c.OrganizationId == organizationId),
            OrderBy  = q => q.OrderBy(c => c.Name),
            Selector = c => new SelectListDto { Id = c.Id, Name = c.ShortName }
        };
        return (await _counterpartyQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetProductGroupsAsync(int? organizationId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<ProductGroup, SelectListDto>
        {
            Criteria = g => g.StateId == StateIdConst.ACTIVE && (organizationId == null || g.OrganizationId == organizationId),
            OrderBy  = q => q.OrderBy(g => g.Name),
            Selector = g => new SelectListDto { Id = g.Id, Name = g.Name, Code = g.Code }
        };
        return (await _productGroupQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetProductsAsync(int? organizationId = null, int? productGroupId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Product, SelectListDto>
        {
            Criteria = p => p.StateId == StateIdConst.ACTIVE &&
                            (organizationId == null || p.OrganizationId == organizationId) &&
                            (productGroupId == null || p.ProductGroupId == productGroupId),
            OrderBy  = q => q.OrderBy(p => p.Name),
            Selector = p => new SelectListDto { Id = p.Id, Name = p.Name, Code = p.Code }
        };
        return (await _productQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetWarehousesAsync(int? organizationId = null, int? branchId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Warehouse, SelectListDto>
        {
            Criteria = w => w.StateId == StateIdConst.ACTIVE &&
                            (organizationId == null || w.OrganizationId == organizationId) &&
                            (branchId == null || w.BranchId == branchId),
            OrderBy  = q => q.OrderBy(w => w.Name),
            Selector = w => new SelectListDto { Id = w.Id, Name = w.Name, Code = w.Code }
        };
        return (await _warehouseQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetChartAccountsAsync(int? organizationId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<ChartAccount, SelectListDto>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            (organizationId == null || x.OrganizationId == organizationId),
            OrderBy  = q => q.OrderBy(x => x.Code),
            Selector = x => new SelectListDto { Id = x.Id, Name = x.Name, Code = x.Code }
        };
        return (await _chartAccountQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetOrgBankAccountsAsync(int? organizationId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<OrgBankAccount, SelectListDto>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            (organizationId == null || x.OrganizationId == organizationId),
            OrderBy  = q => q.OrderBy(x => x.Name),
            Selector = x => new SelectListDto { Id = x.Id, Name = x.AccountNumber, Code = x.AccountNumber }
        };
        return (await _orgBankAccountQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetCashBoxesAsync(int? organizationId = null, int? branchId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<CashBox, SelectListDto>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            (organizationId == null || x.OrganizationId == organizationId) &&
                            (branchId == null || x.BranchId == branchId),
            OrderBy  = q => q.OrderBy(x => x.Name),
            Selector = x => new SelectListDto { Id = x.Id, Name = x.Name, Code = x.Code }
        };
        return (await _cashBoxQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetCashOperationsAsync(int? organizationId = null, int? cashBoxId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<CashOperation, SelectListDto>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            (organizationId == null || x.OrganizationId == organizationId) &&
                            (cashBoxId == null || x.CashBoxId == cashBoxId),
            OrderBy  = q => q.OrderBy(x => x.Name),
            Selector = x => new SelectListDto { Id = x.Id, Name = x.DocNumber, Code = x.DocNumber }
        };
        return (await _cashOperationQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetLanguagesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Language, SelectListDto>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(x => x.Name),
            Selector = x => new SelectListDto { Id = x.Id, Name = x.Name, Code = x.Code }
        };
        return (await _languageQuery.GetAllAsync(spec, ct)).ToList();
    }

    // ------------------------------------------------------------------
    //  Module sub-groups with their modules (permission tree for UI)
    // ------------------------------------------------------------------
    public async Task<List<ModuleSubGroupSelectListDto>> GetModuleSubGroupSelectListAsync(CancellationToken ct = default)
    {
        // Load all active modules with sub-group info as a flat projection
        var spec = new QuerySpecification<Module, ModuleFlatDto>
        {
            Criteria = m => m.StateId == StateIdConst.ACTIVE,
            OrderBy  = q => q.OrderBy(m => m.SubGroupFullName).ThenBy(m => m.ModuleId),
            Selector = m => new ModuleFlatDto
            {
                SubGroupId        = m.SubGroupId,
                SubGroupCode      = m.SubGroup.Code,
                SubGroupShortName = m.SubGroup.ShortName,
                SubGroupFullName  = m.SubGroup.FullName,
                ModuleId          = m.Id,
                ModuleCode        = m.Code,
                ModuleShortName   = m.ShortName,
                ModuleFullName    = m.FullName
            }
        };

        var flat = await _moduleQuery.GetAllAsync(spec, ct);

        return flat
            .GroupBy(x => new { x.SubGroupId, x.SubGroupCode, x.SubGroupShortName, x.SubGroupFullName })
            .Select(g => new ModuleSubGroupSelectListDto
            {
                Id        = g.Key.SubGroupId,
                Code      = g.Key.SubGroupCode,
                ShortName = g.Key.SubGroupShortName,
                FullName  = g.Key.SubGroupFullName,
                Modules   = g.Select(m => new ModuleSelectListDto
                {
                    Id        = m.ModuleId,
                    Code      = m.ModuleCode,
                    ShortName = m.ModuleShortName,
                    FullName  = m.ModuleFullName
                }).ToList()
            })
            .OrderBy(sg => sg.FullName)
            .ToList();
    }

    // Private flat projection DTO (only used inside ManualService)
    private sealed class ModuleFlatDto
    {
        public int    SubGroupId        { get; init; }
        public string SubGroupCode      { get; init; } = null!;
        public string SubGroupShortName { get; init; } = null!;
        public string SubGroupFullName  { get; init; } = null!;
        public int    ModuleId          { get; init; }
        public string ModuleCode        { get; init; } = null!;
        public string ModuleShortName   { get; init; } = null!;
        public string ModuleFullName    { get; init; } = null!;
    }
}
