using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.FaAssets;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Manual;

public class ManualService : IManualService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Bank> _bankQuery;
    private readonly IQueryRepository<BankTerminal> _bankTerminalQuery;
    private readonly IQueryRepository<PaymentMethod> _paymentMethodQuery;
    private readonly IQueryRepository<Role> _roleQuery;
    private readonly IQueryRepository<UserKind> _userKindQuery;
    private readonly IQueryRepository<User> _userQuery;
    private readonly IQueryRepository<Unit> _unitQuery;
    private readonly IQueryRepository<State> _stateQuery;
    private readonly IQueryRepository<Branch> _branchQuery;
    private readonly IQueryRepository<FaOkof> _faOkofQuery;
    private readonly IQueryRepository<Module> _moduleQuery;
    private readonly IQueryRepository<Region> _regionQuery;
    private readonly IQueryRepository<FaGroup> _faGroupQuery;
    private readonly IQueryRepository<TaxType> _taxTypeQuery;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<District> _districtQuery;
    private readonly IQueryRepository<PaymentType> _paymentTypeQuery;
    private readonly IQueryRepository<DocumentStatus> _documentStatusQuery;
    private readonly IQueryRepository<CounterpartyType> _counterpartyTypeQuery;
    private readonly IQueryRepository<InventoryAdjustmentType> _inventoryAdjustmentTypeQuery;
    private readonly IQueryRepository<FaDepreciationMethod> _faDepreciationMethodQuery;
    private readonly IQueryRepository<FaReceiptType> _faReceiptTypeQuery;
    private readonly IQueryRepository<FaDisposalType> _faDisposalTypeQuery;
    private readonly IQueryRepository<FaAsset> _faAssetQuery;
    private readonly IQueryRepository<PriceRoundingMethod> _priceRoundingMethodQuery;
    private readonly IQueryRepository<PricingMethod> _pricingMethodQuery;
    private readonly IQueryRepository<CostingMethod> _costingMethodQuery;
    private readonly IQueryRepository<DocumentType> _documentTypeQuery;
    private readonly IQueryRepository<OperationType> _operationTypeQuery;
    private readonly IQueryRepository<ContractType> _contractTypeQuery;
    private readonly IQueryRepository<Position> _positionQuery;
    private readonly IQueryRepository<Department> _departmentQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<ProductGroup> _productGroupQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
    private readonly IQueryRepository<AccountingPolicy> _accountingPolicyQuery;
    private readonly IQueryRepository<BankAccount> _orgBankAccountQuery;
    private readonly IQueryRepository<CounterpartyBankAccount> _counterpartyBankAccountQuery;
    private readonly IQueryRepository<CashBox> _cashBoxQuery;
    private readonly IQueryRepository<FiscalCashRegister> _fiscalCashRegisterQuery;
    private readonly IQueryRepository<FiscalCashRegisterType> _fiscalCashRegisterTypeQuery;
    private readonly IQueryRepository<CashOperation> _cashOperationQuery;
    private readonly IQueryRepository<Contract> _contractQuery;
    private readonly IQueryRepository<Language> _languageQuery;
    private readonly IQueryRepository<AccountType> _accountTypeQuery;
    private readonly IQueryRepository<SubkontoType> _subkontoTypeQuery;
    private readonly IQueryBuilder _queryBuilder;
    public ManualService(
        IQueryRepository<Role> roleQuery,
        IQueryRepository<UserKind> userKindQuery,
        IQueryRepository<State> stateQuery,
        IQueryRepository<Region> regionQuery,
        IQueryRepository<District> districtQuery,
        IQueryRepository<User> userQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<Unit> unitQuery,
        IQueryRepository<DocumentStatus> documentStatusQuery,
        IQueryRepository<CounterpartyType> counterpartyTypeQuery,
        IQueryRepository<PaymentType> paymentTypeQuery,
        IQueryRepository<InventoryAdjustmentType> inventoryAdjustmentTypeQuery,
        IQueryRepository<FaGroup> faGroupQuery,
        IQueryRepository<FaOkof> faOkofQuery,
        IQueryRepository<FaDepreciationMethod> faDepreciationMethodQuery,
        IQueryRepository<FaReceiptType> faReceiptTypeQuery,
        IQueryRepository<FaDisposalType> faDisposalTypeQuery,
        IQueryRepository<FaAsset> faAssetQuery,
        IQueryRepository<PriceRoundingMethod> priceRoundingMethodQuery,
        IQueryRepository<PricingMethod> pricingMethodQuery,
        IQueryRepository<CostingMethod> costingMethodQuery,
        IQueryRepository<Bank> bankQuery,
        IQueryRepository<BankTerminal> bankTerminalQuery,
        IQueryRepository<PaymentMethod> paymentMethodQuery,
        IQueryRepository<DocumentType> documentTypeQuery,
        IQueryRepository<OperationType> operationTypeQuery,
        IQueryRepository<TaxType> taxTypeQuery,
        IQueryRepository<VatRate> vatRateQuery,
        IQueryRepository<ContractType> contractTypeQuery,
        IQueryRepository<Branch> branchQuery,
        IQueryRepository<Department> departmentQuery,
        IQueryRepository<Position> positionQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<ProductGroup> productGroupQuery,
        IQueryRepository<Product> productQuery,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<ChartAccount> chartAccountQuery,
        IQueryRepository<AccountingPolicy> accountingPolicyQuery,
        IQueryRepository<BankAccount> orgBankAccountQuery,
        IQueryRepository<CashBox> cashBoxQuery,
        IQueryRepository<FiscalCashRegister> fiscalCashRegisterQuery,
        IQueryRepository<FiscalCashRegisterType> fiscalCashRegisterTypeQuery,
        IQueryRepository<CashOperation> cashOperationQuery,
        IQueryRepository<CounterpartyBankAccount> counterpartyBankAccountQuery,
        IQueryRepository<Contract> contractQuery,
        IQueryRepository<Language> languageQuery,
        IQueryRepository<Organization> organizationQuery,
        IQueryRepository<AccountType> accountTypeQuery,
        IQueryRepository<Module> moduleQuery,
        IQueryRepository<SubkontoType> subkontoTypeQuery,
        IQueryBuilder queryBuilder,
        IUserContext userContext)
    {
        _roleQuery = roleQuery;
        _userKindQuery = userKindQuery;
        _stateQuery = stateQuery;
        _regionQuery = regionQuery;
        _districtQuery = districtQuery;
        _userQuery = userQuery;
        _currencyQuery = currencyQuery;
        _unitQuery = unitQuery;
        _documentStatusQuery = documentStatusQuery;
        _counterpartyTypeQuery = counterpartyTypeQuery;
        _paymentTypeQuery = paymentTypeQuery;
        _inventoryAdjustmentTypeQuery = inventoryAdjustmentTypeQuery;
        _faGroupQuery = faGroupQuery;
        _faOkofQuery = faOkofQuery;
        _faDepreciationMethodQuery = faDepreciationMethodQuery;
        _faReceiptTypeQuery = faReceiptTypeQuery;
        _faDisposalTypeQuery = faDisposalTypeQuery;
        _faAssetQuery = faAssetQuery;
        _priceRoundingMethodQuery = priceRoundingMethodQuery;
        _pricingMethodQuery = pricingMethodQuery;
        _costingMethodQuery = costingMethodQuery;
        _bankQuery = bankQuery;
        _bankTerminalQuery = bankTerminalQuery;
        _paymentMethodQuery = paymentMethodQuery;
        _documentTypeQuery = documentTypeQuery;
        _operationTypeQuery = operationTypeQuery;
        _taxTypeQuery = taxTypeQuery;
        _vatRateQuery = vatRateQuery;
        _contractTypeQuery = contractTypeQuery;
        _branchQuery = branchQuery;
        _departmentQuery = departmentQuery;
        _positionQuery = positionQuery;
        _counterpartyQuery = counterpartyQuery;
        _productGroupQuery = productGroupQuery;
        _productQuery = productQuery;
        _warehouseQuery = warehouseQuery;
        _chartAccountQuery = chartAccountQuery;
        _accountingPolicyQuery = accountingPolicyQuery;
        _orgBankAccountQuery = orgBankAccountQuery;
        _subkontoTypeQuery = subkontoTypeQuery;
        _cashBoxQuery = cashBoxQuery;
        _fiscalCashRegisterQuery = fiscalCashRegisterQuery;
        _fiscalCashRegisterTypeQuery = fiscalCashRegisterTypeQuery;
        _cashOperationQuery = cashOperationQuery;
        _contractQuery = contractQuery;
        _languageQuery = languageQuery;
        _organizationQuery = organizationQuery;
        _moduleQuery = moduleQuery;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _accountTypeQuery = accountTypeQuery;
        _counterpartyBankAccountQuery = counterpartyBankAccountQuery;
    }

    public async Task<List<SelectListDto>> GetStatesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<State>()
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.FullName
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();
        
        return await _stateQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetRegionsAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Region>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.FullName
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _regionQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetDistrictsAsync(int? regionId = null, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<District>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE &&
                                            (regionId == null || x.RegionId == regionId))
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.FullName
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _districtQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Currency>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _currencyQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetUnitsAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Unit>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _unitQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetDocumentStatusesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<DocumentStatus>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _documentStatusQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetCounterpartyTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<CounterpartyType, SelectListDto>
        {
            Criteria = c => c.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(c => c.Name),
            Selector = c => new SelectListDto { Id = c.Id, Name = c.Name, Code = c.Code }
        };
        return (await _counterpartyTypeQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetPaymentTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<PaymentType, SelectListDto>
        {
            Criteria = p => p.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(p => p.Name),
            Selector = p => new SelectListDto { Id = p.Id, Name = p.Name, Code = p.Code }
        };
        return (await _paymentTypeQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetInventoryAdjustmentTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<InventoryAdjustmentType, SelectListDto>
        {
            Criteria = p => p.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(p => p.Name),
            Selector = p => new SelectListDto { Id = p.Id, Name = p.Name, Code = p.Code }
        };
        return (await _inventoryAdjustmentTypeQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetFaGroupsAsync(CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
        {
            return new List<SelectListDto>();
        }

        var spec = new QuerySpecification<FaGroup, SelectListDto>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            x.OrganizationId == _userContext.OrganizationId,
            OrderBy = q => q.OrderBy(x => x.Name),
            Selector = x => new SelectListDto { Id = x.Id, Name = x.Name, Code = x.Code }
        };
        return (await _faGroupQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetFaOkofsAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<FaOkof, SelectListDto>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(x => x.Name),
            Selector = x => new SelectListDto { Id = x.Id, Name = x.Name, Code = x.Code }
        };
        return (await _faOkofQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetFaDepreciationMethodsAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<FaDepreciationMethod, SelectListDto>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(x => x.Name),
            Selector = x => new SelectListDto { Id = x.Id, Name = x.Name, Code = x.Code }
        };
        return (await _faDepreciationMethodQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetFaReceiptTypesAsync(CancellationToken ct = default)
    {
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var query = _queryBuilder.For<FaReceiptType>()
            .As(x => new SelectListDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.FaReceiptTypeTranslations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? x.Name
            })
            .OrderBy(x => x.Name)
            .Build();

        return await _faReceiptTypeQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetFaDisposalTypesAsync(CancellationToken ct = default)
    {
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var query = _queryBuilder.For<FaDisposalType>()
            .As(x => new SelectListDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.FaDisposalTypeTranslations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? x.Name
            })
            .OrderBy(x => x.Name)
            .Build();

        return await _faDisposalTypeQuery.GetAllAsync(query, ct);
    }

    public async Task<List<FaAssetSelectListDto>> GetFaAssetsAsync(FaAssetListFilter filter, CancellationToken ct = default)
    {
        var search = filter.Search?.Trim().ToLower();

        var query = _queryBuilder.For<FaAsset>()
            .Where(x => x.StateId == StateIdConst.ACTIVE &&
                        (!filter.FaGroupId.HasValue || x.FaGroupId == filter.FaGroupId.Value) &&
                        (!filter.StatusId.HasValue || x.StatusId == filter.StatusId.Value) &&
                        (string.IsNullOrEmpty(search) ||
                         x.InventoryNumber.ToLower().Contains(search) ||
                         x.Name.ToLower().Contains(search)))
            .As(x => new FaAssetSelectListDto
            {
                Id = x.Id,
                Code = x.InventoryNumber,
                InventoryNumber = x.InventoryNumber,
                Name = x.Name,
                FaGroupId = x.FaGroupId,
                FaGroupName = x.FaGroup.Name,
                InitialCost = x.FaAssetAccounting != null ? x.FaAssetAccounting.InitialCost : 0m,
                AssetAccountId = x.FaAssetAccounting != null
                    ? x.FaAssetAccounting.AssetAccountId
                    : null,
                AssetAccountNumber = x.FaAssetAccounting != null
                    ? x.FaAssetAccounting.AssetAccount.Number
                    : null,
                AssetAccountName = x.FaAssetAccounting != null
                    ? x.FaAssetAccounting.AssetAccount.Name
                    : null,
                StatusId = x.StatusId,
                StatusName = x.Status.Name
            })
            .OrderBy(x => x.InventoryNumber)
            .Build();

        return await _faAssetQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetPriceRoundingMethodsAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<PriceRoundingMethod>()
            .As(x => new SelectListDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .OrderBy(x => x.Name)
            .Build();

        return await _priceRoundingMethodQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetPricingMethodsAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<PricingMethod, SelectListDto>
        {
            Criteria = x => true,
            OrderBy = q => q.OrderBy(x => x.Name),
            Selector = x => new SelectListDto { Id = x.Id, Name = x.Name, Code = x.Code }
        };
        return await _pricingMethodQuery.GetAllAsync(spec, ct);
    }

    public async Task<List<SelectListDto>> GetCostingMethodsAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<CostingMethod, SelectListDto>
        {
            Criteria = x => true,
            OrderBy = q => q.OrderBy(x => x.Name),
            Selector = x => new SelectListDto { Id = x.Id, Name = x.Name, Code = x.Code }
        };
        return await _costingMethodQuery.GetAllAsync(spec, ct);
    }

    public async Task<List<SelectListDto>> GetRolesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Role, SelectListDto>
        {
            Criteria = r => r.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(r => r.Name),
            Selector = r => new SelectListDto { Id = r.Id, Name = r.FullName }
        };
        return (await _roleQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetUserKindsAsync(CancellationToken ct = default)
    {
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var query = _queryBuilder.For<UserKind>()
            .As(x => new SelectListDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.UserKindTranslations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? x.Name
            })
            .OrderBy(x => x.Name)
            .Build();

        return await _userKindQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetUsersAsync(int? roleId = null, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<User, SelectListDto>
        {
            Criteria = u => u.StateId == StateIdConst.ACTIVE
                         && (roleId == null || u.UserOrganizations.Any(membership => membership.RoleId == roleId)),
            OrderBy = q => q.OrderBy(u => u.Name),
            Selector = u => new SelectListDto { Id = u.Id, Name = u.FirstName + " " + u.LastName }
        };
        return (await _userQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetBanksAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<Bank, SelectListDto>
        {
            Criteria = b => b.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(b => b.Name),
            Selector = b => new SelectListDto { Id = b.Id, Name = b.Name, Code = b.Code }
        };
        return (await _bankQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetDocumentTypesAsync(CancellationToken ct = default)
    {
        var spec = new QuerySpecification<DocumentType, SelectListDto>
        {
            Criteria = d => d.StateId == StateIdConst.ACTIVE,
            OrderBy = q => q.OrderBy(d => d.Name),
            Selector = d => new SelectListDto { Id = d.Id, Name = d.Name, Code = d.Code }
        };
        return (await _documentTypeQuery.GetAllAsync(spec, ct)).ToList();
    }

    public async Task<List<SelectListDto>> GetOperationTypesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<OperationType>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _operationTypeQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetTaxTypesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<TaxType>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _taxTypeQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetVatRatesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<VatRate>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _vatRateQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetContractTypesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ContractType>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _contractTypeQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetBranchesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Branch>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _branchQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetDepartmentsAsync(int? branchId = null, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Department>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE &&
                                             (branchId == null || x.BranchId == branchId))
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _departmentQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetPositionsAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Position>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _positionQuery.GetAllAsync(query, ct);
    }

    public async Task<Result<List<SelectListDto>>> GetContractsAsync(int? counterpartyId = null, short? contractTypeId = null, DateTime? choosedDate = null, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<List<SelectListDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var date = choosedDate ?? DateTime.Now.Date;
        var endDate = date.AddDays(1).AddTicks(-1);

        var query = _queryBuilder.For<Contract>()
                        .Where(x => x.StateId == StateIdConst.ACTIVE &&
                                    x.OrganizationId == _userContext.OrganizationId &&
                                    (x.StartDate == null || x.StartDate <= date) &&
                                    (x.EndDate == null || x.EndDate >= endDate) &&
                                    (counterpartyId == null || x.CounterpartyId == counterpartyId) &&
                                    (contractTypeId == null || x.ContractTypeId == contractTypeId))
                        .As(a => new SelectListDto
                        {
                            Id = a.Id,
                            Name = a.ContractNumber
                        })
                        .Build();

        return await _contractQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetCounterpartiesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CounterpartyCard>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.ShortName!
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _counterpartyQuery.GetAllAsync(query, ct);
    }

    public async Task<List<CounterpartySelectListDto>> GetSuppliersAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CounterpartyCard>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE &&
                                             (x.CounterpartyTypeId == CounterPartyTypeIdConst.CLIENT_SUPPLIER ||
                                              x.CounterpartyTypeId == CounterPartyTypeIdConst.SUPPLIER))
                                 .As(s => new CounterpartySelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.FullName!,
                                     Inn = s.Inn
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _counterpartyQuery.GetAllAsync(query, ct);
    }

    public async Task<List<CounterpartySelectListDto>> GetClientsAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CounterpartyCard>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE &&
                                             (x.CounterpartyTypeId == CounterPartyTypeIdConst.CLIENT_SUPPLIER ||
                                              x.CounterpartyTypeId == CounterPartyTypeIdConst.CLIENT))
                                 .As(s => new CounterpartySelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.FullName!,
                                     Inn = s.Inn
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _counterpartyQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetProductGroupsAsync(CancellationToken ct = default)
    {
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var query = _queryBuilder.For<ProductGroup>()
            .Where(group => group.StateId == StateIdConst.ACTIVE && group.IsAssignable)
            .As(group => new SelectListDto
            {
                Id = group.Id,
                Name = group.ProductGroupTranslations
                    .Where(translation => translation.LanguageId == languageId)
                    .Select(translation => translation.Name)
                    .FirstOrDefault() ?? group.Name,
                Code = group.Code
            })
            .OrderBy(item => item.Name)
            .Build();

        return await _productGroupQuery.GetAllAsync(query, ct);
    }

    public async Task<List<ProductSelectListDto>> GetProductsAsync(
        int? productGroupId = null,
        int? warehouseId = null,
        bool? isService = null,
        bool? isSold = null,
        bool? isPurchased = null,
        CancellationToken ct = default)
    {

        var query = _queryBuilder.For<Product>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE &&
                                             (productGroupId == null || x.ProductGroupId == productGroupId) &&
                                             (isService == null || x.IsService == isService) &&
                                             (isSold == null || x.IsSold == isSold) &&
                                             (isPurchased == null || x.IsPurchased == isPurchased) &&
                                             (warehouseId == null || x.WarehouseProductMovements.Any(a =>
                                                 a.WarehouseId == warehouseId)))
                                 .As(s => new ProductSelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Barcode,
                                     Mxik = s.Mxik,
                                     UnitId = s.UnitId,
                                     UnitCode = s.Unit.Code,
                                     IsPieceTracked = s.IsPieceTracked,
                                     IsService = s.IsService,
                                     IsSold = s.IsSold,
                                     IsPurchased = s.IsPurchased
                                 }).Build();

        return await _productQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetOrganizationsAsync(CancellationToken ct = default)
    {
        var query = _userContext.UserKind switch
        {
            CurrentUserKind.SuperAdmin =>
                _queryBuilder
                    .For<Organization>()
                    .Where(x => x.StateId == StateIdConst.ACTIVE)
                    .As(x => new SelectListDto
                    {
                        Id = x.Id,
                        Name = x.FullName
                    })
                    .OrderBy(x => x.Name)
                    .Build(),

            CurrentUserKind.TenantAdmin =>
                _queryBuilder
                    .For<Organization>()
                    .Where(x =>
                        x.StateId == StateIdConst.ACTIVE &&
                        x.TenantId == _userContext.TenantId)
                    .As(x => new SelectListDto
                    {
                        Id = x.Id,
                        Name = x.FullName
                    })
                    .OrderBy(x => x.Name)
                    .Build(),

            CurrentUserKind.TenantUser =>
                _queryBuilder
                    .For<Organization>()
                    .Where(x =>
                        x.StateId == StateIdConst.ACTIVE &&
                        x.UserOrganizations.Any(uo =>
                            uo.UserId == _userContext.Id))
                    .As(x => new SelectListDto
                    {
                        Id = x.Id,
                        Name = x.FullName
                    })
                    .OrderBy(x => x.Name)
                    .Build(),

            _ => throw new InvalidOperationException(
                $"Unsupported user kind: {_userContext.UserKind}")
        };

        return await _organizationQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetWarehousesAsync(int? branchId = null, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Warehouse>()
                            .Where(x => x.StateId == StateIdConst.ACTIVE &&
                                        (branchId == null || x.BranchId == branchId))
                            .As(s => new SelectListDto
                            {
                                Id = s.Id,
                                Name = s.Name
                            })
                            .OrderBy(o => o.Name)
                            .Build();

        return await _warehouseQuery.GetAllAsync(query, ct);
    }

    public async Task<List<ChartAccountSelectListDto>> GetChartAccountsAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ChartAccount>()
                            .Where(x => x.StateId == StateIdConst.ACTIVE)
                            .As(s => new ChartAccountSelectListDto
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Code = s.Code,
                                Number = s.Number
                            })
                            .OrderBy(x => x.Number)
                            .Build();

        return await _chartAccountQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetAccountingPoliciesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<AccountingPolicy>()
                            .Where(x => x.StateId == StateIdConst.ACTIVE)
                            .As(s => new SelectListDto
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Code = s.Code
                            })
                            .OrderBy(o => o.Name)
                            .Build();

        return await _accountingPolicyQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetOrgBankAccountsAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<BankAccount>()
                            .Where(x => x.StateId == StateIdConst.ACTIVE)
                            .As(s => new SelectListDto
                            {
                                Id = s.Id,
                                Name = s.AccountNumber,
                                Code = s.AccountNumber
                            })
                            .OrderBy(o => o.Name)
                            .Build();

        return await _orgBankAccountQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetBankTerminalsAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<BankTerminal>()
            .Where(x => x.StateId == StateIdConst.ACTIVE)
            .As(x => new SelectListDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.ExternalTerminalId ?? x.SerialNumber
            })
            .OrderBy(x => x.Name)
            .Build();

        return await _bankTerminalQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetPaymentMethodsAsync(CancellationToken ct = default)
    {
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var query = _queryBuilder.For<PaymentMethod>()
            .As(x => new SelectListDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.PaymentMethodTranslations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? x.Name
            })
            .OrderBy(x => x.Name)
            .Build();

        return await _paymentMethodQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetCounterpartyBankAccountsAsync(int? counterpartyId = null, int? bankId = null, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CounterpartyBankAccount>()
                            .Where(x => x.StateId == StateIdConst.ACTIVE &&
                                        (counterpartyId == null || x.CounterpartyId == counterpartyId) &&
                                        (bankId == null || x.BankId == bankId))
                            .As(s => new SelectListDto
                            {
                                Id = s.Id,
                                Name = s.AccountNumber,
                                Code = s.Bank.Code
                            }).Build();

        return await _counterpartyBankAccountQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetCashBoxesAsync(int? branchId = null, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CashBox>()
                            .Where(x => x.StateId == StateIdConst.ACTIVE &&
                                        (branchId == null || x.BranchId == branchId))
                            .As(s => new SelectListDto
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Code = s.Code
                            })
                            .OrderBy(o => o.Name)
                            .Build();

        return await _cashBoxQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetFiscalCashRegistersAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<FiscalCashRegister>()
            .Where(x => x.StateId == StateIdConst.ACTIVE)
            .As(x => new SelectListDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.ExternalRegisterId ?? x.SerialNumber
            })
            .OrderBy(x => x.Name)
            .Build();

        return await _fiscalCashRegisterQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetFiscalCashRegisterTypesAsync(CancellationToken ct = default)
    {
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var query = _queryBuilder.For<FiscalCashRegisterType>()
            .As(x => new SelectListDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.FiscalCashRegisterTypeTranslations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? x.Name
            })
            .OrderBy(x => x.Name)
            .Build();

        return await _fiscalCashRegisterTypeQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetCashOperationsAsync(int? cashBoxId = null, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CashOperation>()
                            .Where(x => x.StateId == StateIdConst.ACTIVE &&
                                        (cashBoxId == null || x.CashBoxId == cashBoxId))
                            .As(s => new SelectListDto
                            {
                                Id = s.Id,
                                Name = s.DocNumber,
                                Code = s.DocNumber
                            })
                            .OrderBy(o => o.Name)
                            .Build();

        return await _cashOperationQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetLanguagesAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Language>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new SelectListDto
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Code = s.Code
                                 })
                                 .OrderBy(o => o.Name)
                                 .Build();

        return await _languageQuery.GetAllAsync(query, ct);
    }

    // ------------------------------------------------------------------
    //  Module sub-groups with their modules (permission tree for UI)
    // ------------------------------------------------------------------
    public async Task<List<ModuleSubGroupSelectListDto>> GetModuleSubGroupSelectListAsync(CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Module>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(s => new ModuleFlatDto
                                 {
                                     SubGroupId = s.SubGroupId,
                                     SubGroupCode = s.SubGroup.Code,
                                     SubGroupShortName = s.SubGroup.ShortName,
                                     SubGroupFullName = s.SubGroup.FullName,
                                     ModuleId = s.Id,
                                     ModuleCode = s.Code,
                                     ModuleShortName = s.ShortName,
                                     ModuleFullName = s.FullName
                                 })
                                 .OrderBy(o => o.ModuleId)
                                 .Build();

        var flat = await _moduleQuery.GetAllAsync(query, ct);

        return flat
            .GroupBy(x => new { x.SubGroupId, x.SubGroupCode, x.SubGroupShortName, x.SubGroupFullName })
            .Select(g => new ModuleSubGroupSelectListDto
            {
                Id = g.Key.SubGroupId,
                Code = g.Key.SubGroupCode,
                ShortName = g.Key.SubGroupShortName,
                FullName = g.Key.SubGroupFullName,
                Modules = g.Select(m => new ModuleSelectListDto
                {
                    Id = m.ModuleId,
                    Code = m.ModuleCode,
                    ShortName = m.ModuleShortName,
                    FullName = m.ModuleFullName
                }).ToList()
            })
            .OrderBy(sg => sg.FullName)
            .ToList();
    }

    public async Task<List<SelectListDto>> GetSubkontoTypesAsync(CancellationToken ct = default)
    {
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;

        var query = _queryBuilder.For<SubkontoType>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(x => new SelectListDto
                                 {
                                     Id = x.Id,
                                     Code = x.Code,
                                     Name = x.SubkontoTypeTranslations
                                                .Where(t => t.LanguageId == languageId)
                                                .Select(t => t.Name)
                                                .FirstOrDefault() ?? x.Name
                                 })
                                 .OrderBy(x => x.Name).Build();

        return await _subkontoTypeQuery.GetAllAsync(query, ct);
    }

    public async Task<List<SelectListDto>> GetAccountTypesAsync(CancellationToken ct = default)
    {

        var query = _queryBuilder.For<AccountType>()
                                 .Where(x => x.StateId == StateIdConst.ACTIVE)
                                 .As(x => new SelectListDto
                                 {
                                     Id = x.Id,
                                     Code = x.Code,
                                     Name = x.Name
                                 })
                                 .OrderBy(x => x.Name).Build();

        return await _accountTypeQuery.GetAllAsync(query, ct);
    }

    // Private flat projection DTO (only used inside ManualService)
    private sealed class ModuleFlatDto
    {
        public int SubGroupId { get; init; }
        public string SubGroupCode { get; init; } = null!;
        public string SubGroupShortName { get; init; } = null!;
        public string SubGroupFullName { get; init; } = null!;
        public int ModuleId { get; init; }
        public string ModuleCode { get; init; } = null!;
        public string ModuleShortName { get; init; } = null!;
        public string ModuleFullName { get; init; } = null!;
    }
}
