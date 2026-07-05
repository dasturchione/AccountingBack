using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupCore : IOrganizationSetupCore
{
    private readonly IQueryRepository<OrganizationSetupState> _setupStateQuery;
    private readonly ICommandRepository<OrganizationSetupState> _setupStateCommand;
    private readonly IQueryRepository<OrganizationTaxSetting> _taxSettingQuery;
    private readonly ICommandRepository<OrganizationTaxSetting> _taxSettingCommand;
    private readonly IQueryRepository<OrganizationConfig> _configQuery;
    private readonly ICommandRepository<OrganizationConfig> _configCommand;
    private readonly IQueryRepository<OrganizationDefault> _defaultQuery;
    private readonly ICommandRepository<OrganizationDefault> _defaultCommand;
    private readonly ICommandRepository<Organization> _organizationCommand;

    public OrganizationSetupCore(
        IQueryRepository<OrganizationSetupState> setupStateQuery,
        ICommandRepository<OrganizationSetupState> setupStateCommand,
        IQueryRepository<OrganizationTaxSetting> taxSettingQuery,
        ICommandRepository<OrganizationTaxSetting> taxSettingCommand,
        IQueryRepository<OrganizationConfig> configQuery,
        ICommandRepository<OrganizationConfig> configCommand,
        IQueryRepository<OrganizationDefault> defaultQuery,
        ICommandRepository<OrganizationDefault> defaultCommand,
        ICommandRepository<Organization> organizationCommand)
    {
        _setupStateQuery = setupStateQuery;
        _setupStateCommand = setupStateCommand;
        _taxSettingQuery = taxSettingQuery;
        _taxSettingCommand = taxSettingCommand;
        _configQuery = configQuery;
        _configCommand = configCommand;
        _defaultQuery = defaultQuery;
        _defaultCommand = defaultCommand;
        _organizationCommand = organizationCommand;
    }

    public async Task UpsertTaxSettingsAsync(int organizationId, OrganizationSetupTaxSettingsWriteModel model, CancellationToken ct = default)
    {
        var tax = await GetCurrentTaxSettingAsync(organizationId, ct);
        if (tax is null)
        {
            tax = new OrganizationTaxSetting
            {
                OrganizationId = organizationId,
                CreatedDate = model.CreatedDate
            };
            ApplyTaxSettings(tax, model);
            await _taxSettingCommand.CreateAsync(tax, ct);
            return;
        }

        ApplyTaxSettings(tax, model);
        await _taxSettingCommand.UpdateAsync(tax, ct);
    }

    public async Task UpsertAccountingPolicyAsync(int organizationId, OrganizationSetupAccountingPolicyWriteModel model, CancellationToken ct = default)
    {
        var config = await _configQuery.GetAsync(new QuerySpecification<OrganizationConfig>
        {
            Criteria = x => x.OrganizationId == organizationId
        }, ct);

        if (config is null)
        {
            config = new OrganizationConfig
            {
                OrganizationId = organizationId
            };
            ApplyAccountingPolicy(config, model);
            await _configCommand.CreateAsync(config, ct);
            return;
        }

        ApplyAccountingPolicy(config, model);
        await _configCommand.UpdateAsync(config, ct);
    }

    public async Task UpsertDefaultsAsync(int organizationId, OrganizationSetupDefaultsWriteModel model, CancellationToken ct = default)
    {
        var defaults = await _defaultQuery.GetAsync(new QuerySpecification<OrganizationDefault>
        {
            Criteria = x => x.OrganizationId == organizationId
        }, ct);

        if (defaults is null)
        {
            defaults = new OrganizationDefault
            {
                OrganizationId = organizationId,
                CreatedDate = model.CreatedDate
            };
            ApplyDefaults(defaults, model);
            await _defaultCommand.CreateAsync(defaults, ct);
            return;
        }

        ApplyDefaults(defaults, model);
        await _defaultCommand.UpdateAsync(defaults, ct);
    }

    public async Task SeedWorkspaceSetupAsync(WorkspaceSetupInitializationRequest request, CancellationToken ct = default)
    {
        if (request.HasTax && request.TaxSettings is not null)
            await UpsertTaxSettingsAsync(request.OrganizationId, request.TaxSettings, ct);

        if (request.HasAccounting && request.AccountingPolicy is not null)
            await UpsertAccountingPolicyAsync(request.OrganizationId, request.AccountingPolicy, ct);

        if (request.HasDefaults && request.Defaults is not null)
            await UpsertDefaultsAsync(request.OrganizationId, request.Defaults, ct);

        await _setupStateCommand.CreateAsync(new OrganizationSetupState
        {
            OrganizationId = request.OrganizationId,
            CurrentStep = ResolveWorkspaceCurrentStep(
                organizationCompleted: true,
                taxCompleted: request.HasTax,
                accountingCompleted: request.HasAccounting,
                defaultsCompleted: request.HasDefaults,
                usersCompleted: request.UsersCompleted,
                isCompleted: request.IsCompleted),
            OrganizationCompleted = true,
            TaxCompleted = request.HasTax,
            AccountingCompleted = request.HasAccounting,
            DefaultsCompleted = request.HasDefaults,
            UsersCompleted = request.UsersCompleted,
            IsCompleted = request.IsCompleted,
            CompletedAt = request.IsCompleted ? request.Timestamp : null,
            CreatedDate = request.Timestamp,
            UpdatedDate = request.Timestamp
        }, ct);
    }

    public async Task UpdateSetupStateAsync(int organizationId, Action<OrganizationSetupState> update, CancellationToken ct = default)
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

    public async Task<Result> CompleteSetupAsync(Organization organization, CancellationToken ct = default)
    {
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

    private async Task<OrganizationSetupState> GetOrCreateSetupStateAsync(int organizationId, CancellationToken ct)
    {
        var setup = await _setupStateQuery.GetAsync(new QuerySpecification<OrganizationSetupState>
        {
            Criteria = x => x.OrganizationId == organizationId
        }, ct);

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

    private static void ApplyTaxSettings(OrganizationTaxSetting tax, OrganizationSetupTaxSettingsWriteModel model)
    {
        tax.TaxTypeId = model.TaxTypeId;
        tax.IsVatPayer = model.IsVatPayer;
        tax.VatRegistrationNumber = model.VatRegistrationNumber;
        tax.EffectiveFrom = model.EffectiveFrom;
        tax.EffectiveTo = model.EffectiveTo;
        tax.StateId = model.StateId;
    }

    private static void ApplyAccountingPolicy(OrganizationConfig config, OrganizationSetupAccountingPolicyWriteModel model)
    {
        config.InventoryValuationMethod = model.InventoryValuationMethod;
        config.AccountingPolicyId = model.AccountingPolicyId;
        config.BaseCurrencyId = model.BaseCurrencyId;
        config.AccountingStartDate = model.AccountingStartDate;
        config.FiscalYearStartMonth = model.FiscalYearStartMonth <= 0 ? (short)1 : model.FiscalYearStartMonth;
    }

    private static void ApplyDefaults(OrganizationDefault defaults, OrganizationSetupDefaultsWriteModel model)
    {
        defaults.BranchId = model.BranchId;
        defaults.WarehouseId = model.WarehouseId;
        defaults.CashBoxId = model.CashBoxId;
        defaults.BankAccountId = model.BankAccountId;
        defaults.ReceivableAccountId = model.ReceivableAccountId;
        defaults.PayableAccountId = model.PayableAccountId;
        defaults.InventoryAccountId = model.InventoryAccountId;
        defaults.CashAccountId = model.CashAccountId;
        defaults.BankAccountingAccountId = model.BankAccountingAccountId;
        defaults.RevenueAccountId = model.RevenueAccountId;
        defaults.ExpenseAccountId = model.ExpenseAccountId;
        defaults.CogsAccountId = model.CogsAccountId;
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

    private static string ResolveWorkspaceCurrentStep(
        bool organizationCompleted,
        bool taxCompleted,
        bool accountingCompleted,
        bool defaultsCompleted,
        bool usersCompleted,
        bool isCompleted)
    {
        if (isCompleted)
            return "complete";
        if (!organizationCompleted)
            return "company-profile";
        if (!taxCompleted)
            return "tax-settings";
        if (!accountingCompleted)
            return "accounting-policy";
        if (!defaultsCompleted)
            return "defaults";
        if (!usersCompleted)
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
