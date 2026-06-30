using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupService : BaseService, IOrganizationSetupService
{
    private static readonly HashSet<string> InventoryValuationMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "fifo",
        "lifo",
        "average"
    };

    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly ICommandRepository<Organization> _organizationCommand;
    private readonly IQueryRepository<OrganizationSetupState> _setupStateQuery;
    private readonly ICommandRepository<OrganizationSetupState> _setupStateCommand;
    private readonly IQueryRepository<OrganizationTaxSetting> _taxSettingQuery;
    private readonly ICommandRepository<OrganizationTaxSetting> _taxSettingCommand;
    private readonly IQueryRepository<OrganizationConfig> _configQuery;
    private readonly ICommandRepository<OrganizationConfig> _configCommand;
    private readonly IQueryRepository<OrganizationDefault> _defaultQuery;
    private readonly ICommandRepository<OrganizationDefault> _defaultCommand;
    private readonly IQueryRepository<UserOrganization> _userOrganizationQuery;
    private readonly IQueryRepository<TaxType> _taxTypeQuery;
    private readonly IQueryRepository<AccountingPolicy> _accountingPolicyQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<Branch> _branchQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<CashBox> _cashBoxQuery;
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;

    public OrganizationSetupService(
        IUserContext userContext,
        IQueryRepository<Organization> organizationQuery,
        ICommandRepository<Organization> organizationCommand,
        IQueryRepository<OrganizationSetupState> setupStateQuery,
        ICommandRepository<OrganizationSetupState> setupStateCommand,
        IQueryRepository<OrganizationTaxSetting> taxSettingQuery,
        ICommandRepository<OrganizationTaxSetting> taxSettingCommand,
        IQueryRepository<OrganizationConfig> configQuery,
        ICommandRepository<OrganizationConfig> configCommand,
        IQueryRepository<OrganizationDefault> defaultQuery,
        ICommandRepository<OrganizationDefault> defaultCommand,
        IQueryRepository<UserOrganization> userOrganizationQuery,
        IQueryRepository<TaxType> taxTypeQuery,
        IQueryRepository<AccountingPolicy> accountingPolicyQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<Branch> branchQuery,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<CashBox> cashBoxQuery,
        IQueryRepository<BankAccount> bankAccountQuery,
        IQueryRepository<ChartAccount> chartAccountQuery,
        ILogger<OrganizationSetupService> logger,
        IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _organizationQuery = organizationQuery;
        _organizationCommand = organizationCommand;
        _setupStateQuery = setupStateQuery;
        _setupStateCommand = setupStateCommand;
        _taxSettingQuery = taxSettingQuery;
        _taxSettingCommand = taxSettingCommand;
        _configQuery = configQuery;
        _configCommand = configCommand;
        _defaultQuery = defaultQuery;
        _defaultCommand = defaultCommand;
        _userOrganizationQuery = userOrganizationQuery;
        _taxTypeQuery = taxTypeQuery;
        _accountingPolicyQuery = accountingPolicyQuery;
        _currencyQuery = currencyQuery;
        _branchQuery = branchQuery;
        _warehouseQuery = warehouseQuery;
        _cashBoxQuery = cashBoxQuery;
        _bankAccountQuery = bankAccountQuery;
        _chartAccountQuery = chartAccountQuery;
    }

    public Task<Result<OrganizationSetupDto>> GetAsync(CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAsync), async () =>
        {
            var orgIdResult = await ResolveOrganizationIdAsync(ct);
            if (!orgIdResult.IsSuccess)
                return Result.Failure<OrganizationSetupDto>(orgIdResult.Error);

            var organization = await GetOrganizationAsync(orgIdResult.Value, ct);
            if (organization is null)
                return Result.Failure<OrganizationSetupDto>(OrganizationSetupErrors.OrganizationNotFound(orgIdResult.Value));

            var setup = await GetSetupStateAsync(organization.Id, ct);
            var tax = await GetCurrentTaxSettingAsync(organization.Id, ct);
            var config = await GetConfigAsync(organization.Id, ct);
            var defaults = await GetDefaultsAsync(organization.Id, ct);
            var users = await GetUsersAsync(organization.Id, ct);

            return new OrganizationSetupDto
            {
                OrganizationId = organization.Id,
                SetupStatus = organization.SetupStatus,
                CurrentStep = setup?.CurrentStep ?? "company-profile",
                OrganizationCompleted = setup?.OrganizationCompleted ?? false,
                TaxCompleted = setup?.TaxCompleted ?? false,
                AccountingCompleted = setup?.AccountingCompleted ?? false,
                DefaultsCompleted = setup?.DefaultsCompleted ?? false,
                UsersCompleted = setup?.UsersCompleted ?? false,
                IsCompleted = setup?.IsCompleted ?? false,
                CompletedAt = setup?.CompletedAt,
                CompanyProfile = MapCompanyProfile(organization),
                TaxSettings = tax is null ? null : MapTaxSettings(tax),
                AccountingPolicy = config is null ? null : MapAccountingPolicy(config),
                Defaults = defaults is null ? null : MapDefaults(defaults),
                Users = users
            };
        });

    public Task<Result> UpdateCompanyProfileAsync(OrganizationSetupCompanyProfileDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateCompanyProfileAsync), async () =>
        {
            var orgIdResult = await ResolveOrganizationIdAsync(ct);
            if (!orgIdResult.IsSuccess)
                return Result.Failure(orgIdResult.Error);

            var organization = await GetOrganizationAsync(orgIdResult.Value, ct);
            if (organization is null)
                return Result.Failure(OrganizationSetupErrors.OrganizationNotFound(orgIdResult.Value));

            if (!string.Equals(organization.Inn, dto.Inn, StringComparison.OrdinalIgnoreCase))
            {
                var innExists = await _organizationQuery.AnyAsync(x => x.Id != organization.Id && x.Inn == dto.Inn, ct);
                if (innExists)
                    return Result.Failure(OrganizationSetupErrors.InnConflict(dto.Inn));
            }

            organization.ShortName = dto.ShortName.Trim();
            organization.FullName = dto.FullName.Trim();
            organization.Inn = dto.Inn.Trim();
            organization.PhoneNumber = dto.PhoneNumber;
            organization.RegionId = dto.RegionId;
            organization.DistrictId = dto.DistrictId;
            organization.Address = dto.Address;
            organization.Director = dto.Director;
            organization.DefaultLanguageId = dto.DefaultLanguageId;
            organization.Email = dto.Email;
            organization.Website = dto.Website;
            organization.Oked = dto.Oked;
            organization.SetupStatus = organization.SetupStatus == "completed" ? organization.SetupStatus : "pending";

            await _organizationCommand.UpdateAsync(organization, ct);
            await UpdateSetupStateAsync(organization.Id, setup => setup.OrganizationCompleted = true, ct);
            return Result.Success();
        }, ct);

    public Task<Result> UpdateTaxSettingsAsync(OrganizationSetupTaxSettingsDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateTaxSettingsAsync), async () =>
        {
            var orgIdResult = await ResolveOrganizationIdAsync(ct);
            if (!orgIdResult.IsSuccess)
                return Result.Failure(orgIdResult.Error);

            var taxTypeExists = await _taxTypeQuery.AnyAsync(x => x.Id == dto.TaxTypeId && x.StateId == StateIdConst.ACTIVE, ct);
            if (!taxTypeExists)
                return Result.Failure(OrganizationSetupErrors.TaxTypeNotFound(dto.TaxTypeId));

            var now = DateTime.Now;
            var tax = await GetCurrentTaxSettingAsync(orgIdResult.Value, ct);
            if (tax is null)
            {
                tax = new OrganizationTaxSetting
                {
                    OrganizationId = orgIdResult.Value,
                    CreatedDate = now
                };
                ApplyTaxSettings(tax, dto);
                await _taxSettingCommand.CreateAsync(tax, ct);
            }
            else
            {
                ApplyTaxSettings(tax, dto);
                await _taxSettingCommand.UpdateAsync(tax, ct);
            }

            await UpdateSetupStateAsync(orgIdResult.Value, setup => setup.TaxCompleted = dto.StateId == StateIdConst.ACTIVE, ct);
            return Result.Success();
        }, ct);

    public Task<Result> UpdateAccountingPolicyAsync(OrganizationSetupAccountingPolicyDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAccountingPolicyAsync), async () =>
        {
            var orgIdResult = await ResolveOrganizationIdAsync(ct);
            if (!orgIdResult.IsSuccess)
                return Result.Failure(orgIdResult.Error);

            var method = dto.InventoryValuationMethod.Trim().ToLowerInvariant();
            if (!InventoryValuationMethods.Contains(method))
                return Result.Failure(OrganizationSetupErrors.InvalidInventoryValuationMethod(method));

            var referenceError = await ValidateAccountingReferencesAsync(dto.AccountingPolicyId, dto.BaseCurrencyId, ct);
            if (referenceError is not null)
                return Result.Failure(referenceError);

            var config = await GetConfigAsync(orgIdResult.Value, ct);
            if (config is null)
            {
                config = new OrganizationConfig { OrganizationId = orgIdResult.Value };
                ApplyAccountingPolicy(config, dto, method);
                await _configCommand.CreateAsync(config, ct);
            }
            else
            {
                ApplyAccountingPolicy(config, dto, method);
                await _configCommand.UpdateAsync(config, ct);
            }

            await UpdateSetupStateAsync(orgIdResult.Value, setup => setup.AccountingCompleted = true, ct);
            return Result.Success();
        }, ct);

    public Task<Result> UpdateDefaultsAsync(OrganizationSetupDefaultsDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateDefaultsAsync), async () =>
        {
            var orgIdResult = await ResolveOrganizationIdAsync(ct);
            if (!orgIdResult.IsSuccess)
                return Result.Failure(orgIdResult.Error);

            var validationError = await ValidateDefaultsAsync(orgIdResult.Value, dto, ct);
            if (validationError is not null)
                return Result.Failure(validationError);

            var defaults = await GetDefaultsAsync(orgIdResult.Value, ct);
            if (defaults is null)
            {
                defaults = new OrganizationDefault
                {
                    OrganizationId = orgIdResult.Value,
                    CreatedDate = DateTime.Now
                };
                ApplyDefaults(defaults, dto);
                await _defaultCommand.CreateAsync(defaults, ct);
            }
            else
            {
                ApplyDefaults(defaults, dto);
                await _defaultCommand.UpdateAsync(defaults, ct);
            }

            await UpdateSetupStateAsync(orgIdResult.Value, setup => setup.DefaultsCompleted = true, ct);
            return Result.Success();
        }, ct);

    public Task<Result> UpdateUsersAsync(OrganizationSetupUsersDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateUsersAsync), async () =>
        {
            var orgIdResult = await ResolveOrganizationIdAsync(ct);
            if (!orgIdResult.IsSuccess)
                return Result.Failure(orgIdResult.Error);

            var hasActiveUser = await _userOrganizationQuery.AnyAsync(x =>
                x.OrganizationId == orgIdResult.Value && x.StateId == StateIdConst.ACTIVE, ct);

            if (dto.UsersCompleted && !hasActiveUser)
                return Result.Failure(OrganizationSetupErrors.SetupNotReady("users"));

            await UpdateSetupStateAsync(orgIdResult.Value, setup => setup.UsersCompleted = dto.UsersCompleted, ct);
            return Result.Success();
        }, ct);

    public Task<Result> CompleteAsync(CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CompleteAsync), async () =>
        {
            var orgIdResult = await ResolveOrganizationIdAsync(ct);
            if (!orgIdResult.IsSuccess)
                return Result.Failure(orgIdResult.Error);

            var organization = await GetOrganizationAsync(orgIdResult.Value, ct);
            if (organization is null)
                return Result.Failure(OrganizationSetupErrors.OrganizationNotFound(orgIdResult.Value));

            var setup = await GetOrCreateSetupStateAsync(organization.Id, ct);
            var missingStep = GetMissingStep(setup);
            if (missingStep is not null)
                return Result.Failure(OrganizationSetupErrors.SetupNotReady(missingStep));

            var now = DateTime.Now;
            setup.IsCompleted = true;
            setup.CurrentStep = "complete";
            setup.CompletedAt = now;
            setup.UpdatedDate = now;
            organization.SetupStatus = "completed";
            organization.SetupCompletedAt = now;

            await _setupStateCommand.UpdateAsync(setup, ct);
            await _organizationCommand.UpdateAsync(organization, ct);
            return Result.Success();
        }, ct);

    private async Task<Result<int>> ResolveOrganizationIdAsync(CancellationToken ct)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<int>(OrganizationSetupErrors.OrganizationContextRequired());

        var organizationExists = await _organizationQuery.AnyAsync(x => x.Id == organizationId, ct);
        if (!organizationExists)
            return Result.Failure<int>(OrganizationSetupErrors.OrganizationNotFound(organizationId));

        if (_userContext.HasGlobalAccess)
            return organizationId;

        if (_userContext.Id is null)
            return Result.Failure<int>(OrganizationSetupErrors.Forbidden());

        var membershipExists = await _userOrganizationQuery.AnyAsync(x =>
            x.UserId == _userContext.Id.Value
            && x.OrganizationId == organizationId
            && x.StateId == StateIdConst.ACTIVE
            && x.BlockedAt == null, ct);

        return membershipExists
            ? organizationId
            : Result.Failure<int>(OrganizationSetupErrors.Forbidden());
    }

    private async Task<Organization?> GetOrganizationAsync(int organizationId, CancellationToken ct) =>
        await _organizationQuery.GetAsync(new QuerySpecification<Organization> { Criteria = x => x.Id == organizationId }, ct);

    private async Task<OrganizationSetupState?> GetSetupStateAsync(int organizationId, CancellationToken ct) =>
        await _setupStateQuery.GetAsync(new QuerySpecification<OrganizationSetupState> { Criteria = x => x.OrganizationId == organizationId }, ct);

    private async Task<OrganizationSetupState> GetOrCreateSetupStateAsync(int organizationId, CancellationToken ct)
    {
        var setup = await GetSetupStateAsync(organizationId, ct);
        if (setup is not null)
            return setup;

        var now = DateTime.Now;
        setup = new OrganizationSetupState
        {
            OrganizationId = organizationId,
            CurrentStep = "company-profile",
            CreatedDate = now,
            UpdatedDate = now
        };
        await _setupStateCommand.CreateAsync(setup, ct);
        return setup;
    }

    private async Task UpdateSetupStateAsync(int organizationId, Action<OrganizationSetupState> update, CancellationToken ct)
    {
        var setup = await GetOrCreateSetupStateAsync(organizationId, ct);
        update(setup);
        setup.IsCompleted = setup.OrganizationCompleted
            && setup.TaxCompleted
            && setup.AccountingCompleted
            && setup.DefaultsCompleted
            && setup.UsersCompleted
            && setup.IsCompleted;

        if (!setup.IsCompleted)
        {
            setup.CompletedAt = null;
            setup.CurrentStep = ResolveCurrentStep(setup);
        }

        setup.UpdatedDate = DateTime.Now;
        await _setupStateCommand.UpdateAsync(setup, ct);
    }

    private async Task<OrganizationTaxSetting?> GetCurrentTaxSettingAsync(int organizationId, CancellationToken ct)
    {
        var items = await _taxSettingQuery.GetAllAsync(new QuerySpecification<OrganizationTaxSetting>
        {
            Criteria = x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE,
            OrderBy = query => query.OrderByDescending(x => x.EffectiveFrom)
        }, ct);

        return items.FirstOrDefault();
    }

    private async Task<OrganizationConfig?> GetConfigAsync(int organizationId, CancellationToken ct) =>
        await _configQuery.GetAsync(new QuerySpecification<OrganizationConfig> { Criteria = x => x.OrganizationId == organizationId }, ct);

    private async Task<OrganizationDefault?> GetDefaultsAsync(int organizationId, CancellationToken ct) =>
        await _defaultQuery.GetAsync(new QuerySpecification<OrganizationDefault> { Criteria = x => x.OrganizationId == organizationId }, ct);

    private async Task<List<OrganizationSetupUserDto>> GetUsersAsync(int organizationId, CancellationToken ct) =>
        await _userOrganizationQuery.GetAllAsync(new QuerySpecification<UserOrganization, OrganizationSetupUserDto>
        {
            Criteria = x => x.OrganizationId == organizationId,
            OrderBy = query => query.OrderByDescending(x => x.IsOwner).ThenBy(x => x.UserId),
            Selector = x => new OrganizationSetupUserDto
            {
                UserId = x.UserId,
                UserName = x.User.UserName,
                FullName = x.User.FirstName + " " + x.User.LastName,
                RoleId = x.RoleId,
                RoleName = x.Role != null ? x.Role.FullName : null,
                IsOwner = x.IsOwner,
                IsDefault = x.IsDefault,
                StateId = x.StateId
            }
        }, ct);

    private async Task<Error?> ValidateAccountingReferencesAsync(short? accountingPolicyId, short? baseCurrencyId, CancellationToken ct)
    {
        if (accountingPolicyId.HasValue)
        {
            var exists = await _accountingPolicyQuery.AnyAsync(x => x.Id == accountingPolicyId.Value && x.StateId == StateIdConst.ACTIVE, ct);
            if (!exists)
                return OrganizationSetupErrors.AccountingPolicyNotFound(accountingPolicyId.Value);
        }

        if (baseCurrencyId.HasValue)
        {
            var exists = await _currencyQuery.AnyAsync(x => x.Id == baseCurrencyId.Value && x.StateId == StateIdConst.ACTIVE, ct);
            if (!exists)
                return OrganizationSetupErrors.CurrencyNotFound(baseCurrencyId.Value);
        }

        return null;
    }

    private async Task<Error?> ValidateDefaultsAsync(int organizationId, OrganizationSetupDefaultsDto dto, CancellationToken ct)
    {
        if (dto.BranchId.HasValue && !await _branchQuery.AnyAsync(x => x.Id == dto.BranchId.Value && x.OrganizationId == organizationId, ct))
            return OrganizationSetupErrors.BranchNotFound(dto.BranchId.Value);

        if (dto.WarehouseId.HasValue && !await _warehouseQuery.AnyAsync(x => x.Id == dto.WarehouseId.Value && x.OrganizationId == organizationId, ct))
            return OrganizationSetupErrors.WarehouseNotFound(dto.WarehouseId.Value);

        if (dto.CashBoxId.HasValue && !await _cashBoxQuery.AnyAsync(x => x.Id == dto.CashBoxId.Value && x.OrganizationId == organizationId, ct))
            return OrganizationSetupErrors.CashBoxNotFound(dto.CashBoxId.Value);

        if (dto.BankAccountId.HasValue && !await _bankAccountQuery.AnyAsync(x => x.Id == dto.BankAccountId.Value && x.OrganizationId == organizationId, ct))
            return OrganizationSetupErrors.BankAccountNotFound(dto.BankAccountId.Value);

        var chartAccountIds = new[]
        {
            dto.ReceivableAccountId,
            dto.PayableAccountId,
            dto.InventoryAccountId,
            dto.CashAccountId,
            dto.BankAccountingAccountId,
            dto.RevenueAccountId,
            dto.ExpenseAccountId,
            dto.CogsAccountId
        }.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();

        foreach (var chartAccountId in chartAccountIds)
        {
            var exists = await _chartAccountQuery.AnyAsync(x => x.Id == chartAccountId && x.StateId == StateIdConst.ACTIVE, ct);
            if (!exists)
                return OrganizationSetupErrors.ChartAccountNotFound(chartAccountId);
        }

        return null;
    }

    private static OrganizationSetupCompanyProfileDto MapCompanyProfile(Organization organization) =>
        new()
        {
            ShortName = organization.ShortName,
            FullName = organization.FullName,
            Inn = organization.Inn,
            PhoneNumber = organization.PhoneNumber,
            RegionId = organization.RegionId,
            DistrictId = organization.DistrictId,
            Address = organization.Address,
            Director = organization.Director,
            DefaultLanguageId = organization.DefaultLanguageId,
            Email = organization.Email,
            Website = organization.Website,
            Oked = organization.Oked
        };

    private static OrganizationSetupTaxSettingsDto MapTaxSettings(OrganizationTaxSetting tax) =>
        new()
        {
            TaxTypeId = tax.TaxTypeId,
            IsVatPayer = tax.IsVatPayer,
            VatRegistrationNumber = tax.VatRegistrationNumber,
            EffectiveFrom = tax.EffectiveFrom,
            EffectiveTo = tax.EffectiveTo,
            StateId = tax.StateId
        };

    private static OrganizationSetupAccountingPolicyDto MapAccountingPolicy(OrganizationConfig config) =>
        new()
        {
            InventoryValuationMethod = config.InventoryValuationMethod,
            AccountingPolicyId = config.AccountingPolicyId,
            BaseCurrencyId = config.BaseCurrencyId,
            AccountingStartDate = config.AccountingStartDate,
            FiscalYearStartMonth = config.FiscalYearStartMonth
        };

    private static OrganizationSetupDefaultsDto MapDefaults(OrganizationDefault defaults) =>
        new()
        {
            BranchId = defaults.BranchId,
            WarehouseId = defaults.WarehouseId,
            CashBoxId = defaults.CashBoxId,
            BankAccountId = defaults.BankAccountId,
            ReceivableAccountId = defaults.ReceivableAccountId,
            PayableAccountId = defaults.PayableAccountId,
            InventoryAccountId = defaults.InventoryAccountId,
            CashAccountId = defaults.CashAccountId,
            BankAccountingAccountId = defaults.BankAccountingAccountId,
            RevenueAccountId = defaults.RevenueAccountId,
            ExpenseAccountId = defaults.ExpenseAccountId,
            CogsAccountId = defaults.CogsAccountId
        };

    private static void ApplyTaxSettings(OrganizationTaxSetting tax, OrganizationSetupTaxSettingsDto dto)
    {
        tax.TaxTypeId = dto.TaxTypeId;
        tax.IsVatPayer = dto.IsVatPayer;
        tax.VatRegistrationNumber = dto.VatRegistrationNumber;
        tax.EffectiveFrom = dto.EffectiveFrom;
        tax.EffectiveTo = dto.EffectiveTo;
        tax.StateId = dto.StateId;
    }

    private static void ApplyAccountingPolicy(OrganizationConfig config, OrganizationSetupAccountingPolicyDto dto, string method)
    {
        config.InventoryValuationMethod = method;
        config.AccountingPolicyId = dto.AccountingPolicyId;
        config.BaseCurrencyId = dto.BaseCurrencyId;
        config.AccountingStartDate = dto.AccountingStartDate;
        config.FiscalYearStartMonth = dto.FiscalYearStartMonth <= 0 ? (short)1 : dto.FiscalYearStartMonth;
    }

    private static void ApplyDefaults(OrganizationDefault defaults, OrganizationSetupDefaultsDto dto)
    {
        defaults.BranchId = dto.BranchId;
        defaults.WarehouseId = dto.WarehouseId;
        defaults.CashBoxId = dto.CashBoxId;
        defaults.BankAccountId = dto.BankAccountId;
        defaults.ReceivableAccountId = dto.ReceivableAccountId;
        defaults.PayableAccountId = dto.PayableAccountId;
        defaults.InventoryAccountId = dto.InventoryAccountId;
        defaults.CashAccountId = dto.CashAccountId;
        defaults.BankAccountingAccountId = dto.BankAccountingAccountId;
        defaults.RevenueAccountId = dto.RevenueAccountId;
        defaults.ExpenseAccountId = dto.ExpenseAccountId;
        defaults.CogsAccountId = dto.CogsAccountId;
    }

    private static string ResolveCurrentStep(OrganizationSetupState setup)
    {
        if (!setup.OrganizationCompleted)
            return "company-profile";
        if (!setup.TaxCompleted)
            return "tax-settings";
        if (!setup.AccountingCompleted)
            return "accounting-policy";
        if (!setup.DefaultsCompleted)
            return "defaults";
        if (!setup.UsersCompleted)
            return "users";
        return "complete";
    }

    private static string? GetMissingStep(OrganizationSetupState setup)
    {
        if (!setup.OrganizationCompleted)
            return "company-profile";
        if (!setup.TaxCompleted)
            return "tax-settings";
        if (!setup.AccountingCompleted)
            return "accounting-policy";
        if (!setup.DefaultsCompleted)
            return "defaults";
        if (!setup.UsersCompleted)
            return "users";
        return null;
    }
}
