using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features;
using Application.Features.AccountingPolicies.DTOs;
using Application.Features.AccountingPolicies.Errors;
using Application.Features.Cmn.Taxes;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.AccountingPolicies.Services;

public sealed class AccountingPolicyService : BaseService, IAccountingPolicyService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<OrganizationConfig> _configQuery;
    private readonly IQueryRepository<OrganizationTaxSetting> _taxSettingQuery;
    private readonly IQueryRepository<OrganizationAccountingPolicyVersion> _historyQuery;
    private readonly ICommandRepository<OrganizationAccountingPolicyVersion> _historyCommand;
    private readonly IQueryRepository<AccountingPeriod> _periodQuery;
    private readonly ITaxResolverService _taxResolver;
    private readonly IQueryBuilder _queryBuilder;

    public AccountingPolicyService(
        IUserContext userContext,
        IQueryRepository<OrganizationConfig> configQuery,
        IQueryRepository<OrganizationTaxSetting> taxSettingQuery,
        IQueryRepository<OrganizationAccountingPolicyVersion> historyQuery,
        ICommandRepository<OrganizationAccountingPolicyVersion> historyCommand,
        IQueryRepository<AccountingPeriod> periodQuery,
        ITaxResolverService taxResolver,
        IQueryBuilder queryBuilder,
        ILogger<AccountingPolicyService> logger,
        IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _configQuery = configQuery;
        _taxSettingQuery = taxSettingQuery;
        _historyQuery = historyQuery;
        _historyCommand = historyCommand;
        _periodQuery = periodQuery;
        _taxResolver = taxResolver;
        _queryBuilder = queryBuilder;
    }

    public Task<Result<AccountingPolicyCurrentDto>> GetCurrentAsync(
        DateOnly? effectiveOn,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(GetCurrentAsync), async () =>
        {
            var organizationIdResult = ResolveOrganizationId();
            if (!organizationIdResult.IsSuccess)
                return Result.Failure<AccountingPolicyCurrentDto>(organizationIdResult.Error);

            var date = effectiveOn ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var config = await _configQuery.GetAsync(
                _queryBuilder.For<OrganizationConfig>()
                    .Where(x => x.OrganizationId == organizationIdResult.Value)
                    .Build(), cancellationToken);
            var taxSetting = await GetTaxSettingAsync(organizationIdResult.Value, date, cancellationToken);
            var version = await GetVersionAsync(organizationIdResult.Value, date, cancellationToken);

            return await ComposeCurrentAsync(
                organizationIdResult.Value,
                date,
                config,
                taxSetting,
                version,
                cancellationToken);
        });

    public Task<Result<AccountingPolicyHistoryDto>> GetHistoryAsync(
        DateOnly? dateFrom,
        DateOnly? dateTo,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(GetHistoryAsync), async () =>
        {
            var organizationIdResult = ResolveOrganizationId();
            if (!organizationIdResult.IsSuccess)
                return Result.Failure<AccountingPolicyHistoryDto>(organizationIdResult.Error);

            if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
                return Result.Failure<AccountingPolicyHistoryDto>(AccountingPolicyErrors.InvalidEffectiveDate());

            var versions = await _historyQuery.GetAllAsync(
                _queryBuilder.For<OrganizationAccountingPolicyVersion>()
                    .Where(x => x.OrganizationId == organizationIdResult.Value
                        && (!dateFrom.HasValue || x.EffectiveTo == null || x.EffectiveTo >= dateFrom.Value)
                        && (!dateTo.HasValue || x.EffectiveFrom <= dateTo.Value))
                    .OrderBy(q => q.OrderBy(x => x.EffectiveFrom).ThenBy(x => x.Version))
                    .Build(), cancellationToken);

            return Result.Success(new AccountingPolicyHistoryDto
            {
                OrganizationId = organizationIdResult.Value,
                Items = versions.Select(MapHistoryItem).ToArray(),
                SourceStatus = versions.Count == 0
                    ? AccountingPolicySourceStatus.MISSING
                    : AccountingPolicySourceStatus.CONFIRMED_FROM_LIVE_DATA,
                SourceEvidence = "OrganizationAccountingPolicyVersion snapshots",
                RequiresBusinessDecision = false,
                AffectedModule = "Accounting policy governance",
                ImplementationDependency = "Additive policy history schema"
            });
        });

    public Task<Result<AccountingPolicyImpactDto>> GetImpactAsync(
        DateOnly effectiveOn,
        string? documentType,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(GetImpactAsync), async () =>
        {
            var current = await GetCurrentAsync(effectiveOn, cancellationToken);
            if (!current.IsSuccess)
                return Result.Failure<AccountingPolicyImpactDto>(current.Error);

            return Result.Success(new AccountingPolicyImpactDto
            {
                OrganizationId = current.Value.OrganizationId,
                EffectiveOn = effectiveOn,
                DocumentType = documentType,
                Policy = current.Value,
                ExistingDocumentsRecalculated = false,
                SourceStatus = current.Value.SourceStatus,
                SourceEvidence = "Policy resolution only; existing documents are immutable inputs",
                RequiresBusinessDecision = current.Value.RequiresBusinessDecision,
                AffectedModule = documentType,
                ImplementationDependency = "Consumers must resolve policy at document effective date"
            });
        });

    public Task<Result<AccountingPolicyCurrentDto>> UpdateAsync(
        AccountingPolicyUpdateDto request,
        CancellationToken cancellationToken = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            var organizationIdResult = ResolveOrganizationId();
            if (!organizationIdResult.IsSuccess)
                return Result.Failure<AccountingPolicyCurrentDto>(organizationIdResult.Error);

            var validationError = ValidateUpdate(request);
            if (validationError is not null)
                return Result.Failure<AccountingPolicyCurrentDto>(validationError);

            var effectiveFrom = request.EffectiveFrom!.Value;
            var effectiveTo = request.EffectiveTo;
            var organizationId = organizationIdResult.Value;

            var existingVersions = await _historyQuery.GetAllAsync(
                _queryBuilder.For<OrganizationAccountingPolicyVersion>()
                    .Where(x => x.OrganizationId == organizationId)
                    .OrderBy(q => q.OrderByDescending(x => x.Version))
                    .Build(), cancellationToken);

            if (existingVersions.Any(x => IntervalsOverlap(
                    effectiveFrom, effectiveTo, x.EffectiveFrom, x.EffectiveTo)))
                return Result.Failure<AccountingPolicyCurrentDto>(AccountingPolicyErrors.PolicyVersionOverlap());

            var closedPeriods = await _periodQuery.GetAllAsync(
                _queryBuilder.For<AccountingPeriod>()
                    .Where(x => x.OrganizationId == organizationId && x.IsClosed)
                    .Build(), cancellationToken);

            if (closedPeriods.Any(x => IntervalsOverlap(
                    effectiveFrom, effectiveTo, x.StartDate, x.EndDate)))
                return Result.Failure<AccountingPolicyCurrentDto>(AccountingPolicyErrors.ClosedPeriodProtected());

            if (_userContext.Id is not int userId || userId <= 0)
                return Result.Failure<AccountingPolicyCurrentDto>(AccountingPolicyErrors.UserScopeRequired());

            var nextVersion = existingVersions.Count == 0 ? 1 : existingVersions.Max(x => x.Version) + 1;
            var snapshot = new OrganizationAccountingPolicyVersion
            {
                OrganizationId = organizationId,
                Version = nextVersion,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveTo,
                InventoryValuationMethod = InventoryValuationMethodConst.FIFO,
                BaseCurrencyId = CurrencyIdConst.UZS,
                VatPayer = true,
                TaxTypeId = null,
                VatTaxPeriod = AccountingPolicyVatTaxPeriodConst.MONTH,
                VatBaseMoment = AccountingPolicyVatBaseMomentConst.SHIPMENT,
                ClosedPeriodPolicy = request.ClosedPeriodPolicy
                    ?? AccountingPolicyClosedPeriodPolicyConst.PROTECT_CLOSED_PERIOD,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            await _historyCommand.CreateAsync(snapshot, cancellationToken);

            var config = await _configQuery.GetAsync(
                _queryBuilder.For<OrganizationConfig>()
                    .Where(x => x.OrganizationId == organizationId)
                    .Build(), cancellationToken);
            var taxSetting = await GetTaxSettingAsync(organizationId, effectiveFrom, cancellationToken);

            return await ComposeCurrentAsync(
                organizationId,
                effectiveFrom,
                config,
                taxSetting,
                snapshot,
                cancellationToken);
        }, cancellationToken);

    private Result<int> ResolveOrganizationId()
    {
        return _userContext.OrganizationId is > 0 and int organizationId
            ? organizationId
            : Result.Failure<int>(AccountingPolicyErrors.OrganizationScopeRequired());
    }

    private async Task<OrganizationTaxSetting?> GetTaxSettingAsync(
        int organizationId,
        DateOnly effectiveOn,
        CancellationToken cancellationToken) =>
        await _taxSettingQuery.GetAsync(
            _queryBuilder.For<OrganizationTaxSetting>()
                .Where(x => x.OrganizationId == organizationId
                    && x.StateId == StateIdConst.ACTIVE
                    && x.EffectiveFrom <= effectiveOn
                    && (x.EffectiveTo == null || x.EffectiveTo >= effectiveOn))
                .OrderBy(q => q.OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.Id))
                .Build(), cancellationToken);

    private async Task<OrganizationAccountingPolicyVersion?> GetVersionAsync(
        int organizationId,
        DateOnly effectiveOn,
        CancellationToken cancellationToken) =>
        await _historyQuery.GetAsync(
            _queryBuilder.For<OrganizationAccountingPolicyVersion>()
                .Where(x => x.OrganizationId == organizationId
                    && x.EffectiveFrom <= effectiveOn
                    && (x.EffectiveTo == null || x.EffectiveTo >= effectiveOn))
                .OrderBy(q => q.OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.Version))
                .Build(), cancellationToken);

    private async Task<Result<AccountingPolicyCurrentDto>> ComposeCurrentAsync(
        int organizationId,
        DateOnly effectiveOn,
        OrganizationConfig? config,
        OrganizationTaxSetting? taxSetting,
        OrganizationAccountingPolicyVersion? version,
        CancellationToken cancellationToken)
    {
        var valuation = version?.InventoryValuationMethod ?? config?.InventoryValuationMethod;
        var currencyId = version?.BaseCurrencyId ?? config?.BaseCurrencyId;
        var vatPayer = version?.VatPayer ?? taxSetting?.IsVatPayer;
        var effectiveFrom = version?.EffectiveFrom ?? config?.AccountingStartDate;
        var effectiveTo = version?.EffectiveTo ?? taxSetting?.EffectiveTo;

        var taxResolutionAvailable = false;
        if (taxSetting is not null)
        {
            var taxResolution = await _taxResolver.ResolveAsync(
                organizationId, taxSetting.TaxTypeId, effectiveOn, cancellationToken);
            taxResolutionAvailable = taxResolution.IsSuccess;
        }

        var configStatus = config is null
            ? AccountingPolicySourceStatus.BLOCKED
            : AccountingPolicySourceStatus.CONFIRMED_FROM_LIVE_DATA;
        var taxStatus = taxSetting is null
            ? AccountingPolicySourceStatus.BLOCKED
            : AccountingPolicySourceStatus.CONFIRMED_FROM_LIVE_DATA;
        var validValuation = string.Equals(
            valuation, InventoryValuationMethodConst.FIFO, StringComparison.OrdinalIgnoreCase);
        var validCurrency = currencyId == CurrencyIdConst.UZS;
        var validVatPayer = vatPayer == true;

        var current = new AccountingPolicyCurrentDto
        {
            OrganizationId = organizationId,
            General = new AccountingPolicyGeneralDto
            {
                AccountingStartDate = Confirmed(effectiveFrom, effectiveFrom.HasValue ? configStatus : AccountingPolicySourceStatus.MISSING, "OrganizationConfig.accounting_start_date", "Accounting", "OrganizationConfig"),
                FiscalYearStartMonth = Confirmed(config?.FiscalYearStartMonth, configStatus, "OrganizationConfig.fiscal_year_start_month", "Accounting", "OrganizationConfig")
            },
            Inventory = new AccountingPolicyInventoryDto
            {
                InventoryValuationMethod = validValuation
                    ? Confirmed<string?>(InventoryValuationMethodConst.FIFO, AccountingPolicySourceStatus.CONFIRMED_FROM_PROJECT, "Approved policy contract", "Inventory", "FIFO costing")
                    : Blocked<string?>("OrganizationConfig.inventory_valuation_method is not FIFO", "Inventory", "Approved FIFO contract")
            },
            Currency = new AccountingPolicyCurrencyDto
            {
                BaseCurrencyId = validCurrency
                    ? Confirmed<short?>(CurrencyIdConst.UZS, AccountingPolicySourceStatus.CONFIRMED_FROM_PROJECT, "Approved UZS-only policy", "Currency", "Base currency catalog")
                    : Blocked<short?>("OrganizationConfig.base_currency_id is not approved UZS", "Currency", "Base currency validation"),
                BaseCurrencyCode = validCurrency
                    ? Confirmed<string?>("UZS", AccountingPolicySourceStatus.CONFIRMED_FROM_PROJECT, "Approved UZS-only policy", "Currency", "Currency catalog")
                    : Blocked<string?>("Base currency code cannot be inferred", "Currency", "Currency catalog"),
                CurrencyRevaluationService = OutOfScope<string?>("Foreign currency is outside approved scope", "Currency", "No revaluation activation"),
                RevaluationScope = OutOfScope<string?>("Foreign currency is outside approved scope", "Currency", "No revaluation activation")
            },
            Vat = new AccountingPolicyVatDto
            {
                IsVatPayer = taxSetting is not null && validVatPayer
                    ? Confirmed<bool?>(true, taxStatus, "OrganizationTaxSetting.is_vat_payer", "VAT", "OrganizationTaxSetting")
                    : Blocked<bool?>(taxSetting is null
                        ? "OrganizationTaxSetting is unavailable"
                        : "OrganizationTaxSetting.is_vat_payer is not approved true", "VAT", "Tax schema reconciliation"),
                TaxTypeId = Blocked<short?>("Tax type remains unresolved by approved contract", "VAT", "Business approval and tax schema"),
                VatTaxPeriod = Confirmed<string?>(AccountingPolicyVatTaxPeriodConst.MONTH, AccountingPolicySourceStatus.CONFIRMED_FROM_PROJECT, "Approved monthly VAT policy", "VAT", "VAT period validation"),
                VatBaseMoment = Confirmed<string?>(AccountingPolicyVatBaseMomentConst.SHIPMENT, AccountingPolicySourceStatus.CONFIRMED_FROM_PROJECT, "Approved shipment VAT policy", "VAT", "VAT timing validation"),
                ActiveVatRateCatalog = taxResolutionAvailable
                    ? Confirmed<string?>("AVAILABLE", AccountingPolicySourceStatus.AVAILABLE, "TaxResolverService", "VAT", "TaxResolverService")
                    : Blocked<string?>(taxSetting is null ? "Tax setting is unavailable" : "TaxResolverService could not resolve tax data", "VAT", "TaxResolverService")
            },
            Payroll = new AccountingPolicyPayrollDto
            {
                PayrollComponentModel = OutOfScope<string?>("Advanced payroll tax is outside approved scope", "Payroll", "No payroll policy activation"),
                IndividualTaxPolicy = OutOfScope<string?>("Individual tax is outside approved scope", "Payroll", "No tax formula activation"),
                SocialTaxPolicy = OutOfScope<string?>("Social tax is outside approved scope", "Payroll", "No tax formula activation")
            },
            Production = new AccountingPolicyProductionDto
            {
                ProductionEnabled = OutOfScope<bool?>("Production is outside approved scope", "Production", "No production module activation"),
                ProductionOutputAccountId = OutOfScope<int?>("Production is outside approved scope", "Production", "No production account selection"),
                OutputAccountCode = OutOfScope<string?>("Production is outside approved scope", "Production", "No production account selection")
            },
            Costing = new AccountingPolicyCostingDto
            {
                CostAllocationMethod = OutOfScope<string?>("Overhead allocation is outside approved scope", "Costing", "No overhead allocation")
            },
            Accounts = new AccountingPolicyAccountsDto
            {
                DocumentAccountSettingsCount = NotAvailable<int?>("Document account settings are not part of the policy source query", "Accounts", "Existing document account settings remain unchanged")
            },
            Governance = new AccountingPolicyGovernanceDto
            {
                EffectiveFrom = Confirmed(effectiveFrom, effectiveFrom.HasValue ? configStatus : AccountingPolicySourceStatus.MISSING, "Policy effective interval", "Governance", "Policy history"),
                EffectiveTo = Confirmed(effectiveTo, version is not null ? AccountingPolicySourceStatus.CONFIRMED_FROM_LIVE_DATA : AccountingPolicySourceStatus.NOT_CONFIGURED, "Policy effective interval", "Governance", "Policy history"),
                PolicyVersioning = Confirmed<bool?>(true, AccountingPolicySourceStatus.CONFIRMED_FROM_PROJECT, "Approved policy history requirement", "Governance", "OrganizationAccountingPolicyVersion"),
                ClosedPeriodPolicy = Confirmed<string?>(version?.ClosedPeriodPolicy ?? AccountingPolicyClosedPeriodPolicyConst.PROTECT_CLOSED_PERIOD, AccountingPolicySourceStatus.CONFIRMED_FROM_PROJECT, "Approved closed-period protection", "Governance", "AccountingPeriod")
            },
            InventoryValuationMethod = validValuation ? InventoryValuationMethodConst.FIFO : null,
            BaseCurrencyId = validCurrency ? CurrencyIdConst.UZS : null,
            BaseCurrencyCode = validCurrency ? "UZS" : null,
            AccountingStartDate = config?.AccountingStartDate,
            FiscalYearStartMonth = config?.FiscalYearStartMonth,
            IsVatPayer = taxSetting is not null && validVatPayer ? true : null,
            TaxTypeId = null,
            VatTaxPeriod = AccountingPolicyVatTaxPeriodConst.MONTH,
            VatBaseMoment = AccountingPolicyVatBaseMomentConst.SHIPMENT,
            ProductionEnabled = null,
            ForeignCurrencyEnabled = null,
            ForeignCurrency = null,
            CostAllocationMethod = null,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            PolicyVersioning = true,
            ClosedPeriodPolicy = version?.ClosedPeriodPolicy ?? AccountingPolicyClosedPeriodPolicyConst.PROTECT_CLOSED_PERIOD,
            SourceStatus = AccountingPolicySourceStatus.BLOCKED,
            SourceEvidence = taxSetting is null
                ? "OrganizationConfig available; OrganizationTaxSetting/tax schema unavailable"
                : "OrganizationConfig + OrganizationTaxSetting + TaxResolverService",
            RequiresBusinessDecision = true,
            AffectedModule = "Accounting policy",
            ImplementationDependency = taxSetting is null || !taxResolutionAvailable
                ? "Reconcile tax schema/data before activation"
                : "TaxTypeId business approval remains required"
        };

        return Result.Success(current);
    }

    private static Error? ValidateUpdate(AccountingPolicyUpdateDto request)
    {
        if (request is null)
            return AccountingPolicyErrors.InvalidEffectiveDate();

        if (!string.Equals(request.InventoryValuationMethod, InventoryValuationMethodConst.FIFO, StringComparison.OrdinalIgnoreCase))
            return AccountingPolicyErrors.InvalidValuationMethod();
        if (request.BaseCurrencyId != CurrencyIdConst.UZS)
            return AccountingPolicyErrors.InvalidBaseCurrency();
        if (request.VatPayer != true)
            return AccountingPolicyErrors.InvalidVatPeriod();
        if (!string.Equals(request.VatTaxPeriod, AccountingPolicyVatTaxPeriodConst.MONTH, StringComparison.OrdinalIgnoreCase))
            return AccountingPolicyErrors.InvalidVatPeriod();
        if (!string.Equals(request.VatBaseMoment, AccountingPolicyVatBaseMomentConst.SHIPMENT, StringComparison.OrdinalIgnoreCase))
            return AccountingPolicyErrors.InvalidVatBaseMoment();
        if (request.TaxTypeId.HasValue)
            return AccountingPolicyErrors.TaxTypeRequired();
        if (request.ProductionEnabled.HasValue || request.ForeignCurrencyEnabled.HasValue || request.CostAllocationMethod is not null)
            return AccountingPolicyErrors.OutOfScope();
        if (request.ClosedPeriodPolicy is not null
            && !string.Equals(request.ClosedPeriodPolicy, AccountingPolicyClosedPeriodPolicyConst.PROTECT_CLOSED_PERIOD, StringComparison.OrdinalIgnoreCase))
            return AccountingPolicyErrors.InvalidClosedPeriodPolicy();
        if (!request.EffectiveFrom.HasValue
            || request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom.Value)
            return AccountingPolicyErrors.InvalidEffectiveDate();

        return null;
    }

    private static bool IntervalsOverlap(
        DateOnly firstFrom,
        DateOnly? firstTo,
        DateOnly secondFrom,
        DateOnly? secondTo) =>
        firstFrom <= (secondTo ?? DateOnly.MaxValue)
            && secondFrom <= (firstTo ?? DateOnly.MaxValue);

    private static AccountingPolicyHistoryItemDto MapHistoryItem(OrganizationAccountingPolicyVersion version) =>
        new()
        {
            OrganizationId = version.OrganizationId,
            Version = version.Version,
            EffectiveFrom = version.EffectiveFrom,
            EffectiveTo = version.EffectiveTo,
            InventoryValuationMethod = version.InventoryValuationMethod,
            BaseCurrencyId = version.BaseCurrencyId,
            VatPayer = version.VatPayer,
            TaxTypeId = null,
            VatTaxPeriod = version.VatTaxPeriod,
            VatBaseMoment = version.VatBaseMoment,
            ClosedPeriodPolicy = version.ClosedPeriodPolicy
        };

    private static AccountingPolicyValueDto<T> Confirmed<T>(
        T value, AccountingPolicySourceStatus status, string evidence, string module, string dependency) =>
        new()
        {
            Value = value,
            SourceStatus = status,
            SourceEvidence = evidence,
            RequiresBusinessDecision = false,
            AffectedModule = module,
            ImplementationDependency = dependency
        };

    private static AccountingPolicyValueDto<T> Blocked<T>(string evidence, string module, string dependency) =>
        new()
        {
            Value = default!,
            SourceStatus = AccountingPolicySourceStatus.BLOCKED,
            SourceEvidence = evidence,
            RequiresBusinessDecision = true,
            AffectedModule = module,
            ImplementationDependency = dependency
        };

    private static AccountingPolicyValueDto<T> OutOfScope<T>(string evidence, string module, string dependency) =>
        new()
        {
            Value = default!,
            SourceStatus = AccountingPolicySourceStatus.OUT_OF_SCOPE,
            SourceEvidence = evidence,
            RequiresBusinessDecision = false,
            AffectedModule = module,
            ImplementationDependency = dependency
        };

    private static AccountingPolicyValueDto<T> NotAvailable<T>(string evidence, string module, string dependency) =>
        new()
        {
            Value = default!,
            SourceStatus = AccountingPolicySourceStatus.NOT_AVAILABLE,
            SourceEvidence = evidence,
            RequiresBusinessDecision = false,
            AffectedModule = module,
            ImplementationDependency = dependency
        };
}
