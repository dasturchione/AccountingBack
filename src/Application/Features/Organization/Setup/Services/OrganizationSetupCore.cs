using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupCore : IOrganizationSetupCore
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<OrganizationSetupState> _setupStateQuery;
    private readonly ICommandRepository<OrganizationSetupState> _setupStateCommand;
    private readonly IQueryRepository<OrganizationConfig> _configQuery;
    private readonly ICommandRepository<OrganizationConfig> _configCommand;
    private readonly IQueryRepository<OrganizationDefault> _defaultQuery;
    private readonly ICommandRepository<OrganizationDefault> _defaultCommand;
    private readonly ICommandRepository<Organization> _organizationCommand;
    private readonly IQueryBuilder _queryBuilder;

    public OrganizationSetupCore(
        IUserContext userContext,
        IQueryRepository<OrganizationSetupState> setupStateQuery,
        ICommandRepository<OrganizationSetupState> setupStateCommand,
        IQueryRepository<OrganizationConfig> configQuery,
        ICommandRepository<OrganizationConfig> configCommand,
        IQueryRepository<OrganizationDefault> defaultQuery,
        ICommandRepository<OrganizationDefault> defaultCommand,
        ICommandRepository<Organization> organizationCommand,
        IQueryBuilder queryBuilder)
    {
        _userContext = userContext;
        _setupStateQuery = setupStateQuery;
        _setupStateCommand = setupStateCommand;
        _configQuery = configQuery;
        _configCommand = configCommand;
        _defaultQuery = defaultQuery;
        _defaultCommand = defaultCommand;
        _organizationCommand = organizationCommand;
        _queryBuilder = queryBuilder;
    }

    public async Task UpsertAccountingPolicyAsync(int organizationId, OrganizationSetupAccountingPolicyWriteModel model, CancellationToken ct = default)
    {
        var config = await _configQuery.GetAsync(_queryBuilder.For<OrganizationConfig>()
            .Where(x => x.OrganizationId == organizationId)
            .Build(), ct);

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
        var defaults = await _defaultQuery.GetAsync(_queryBuilder.For<OrganizationDefault>()
            .Where(x => x.OrganizationId == organizationId)
            .Build(), ct);

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

    public async Task UpdateSetupStateAsync(int organizationId, Action<OrganizationSetupState> update, CancellationToken ct = default)
    {
        var setup = await GetOrCreateSetupStateAsync(organizationId, ct);
        update(setup);
        setup.IsCompleted = setup.OrganizationCompleted
            && setup.AccountingCompleted
            && setup.DefaultsCompleted
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
            return Result.Failure(OrganizationSetupErrors.SetupNotReady(missingStep, _userContext.LanguageId));

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

    private async Task<OrganizationSetupState> GetOrCreateSetupStateAsync(int organizationId, CancellationToken ct)
    {
        var setup = await _setupStateQuery.GetAsync(_queryBuilder.For<OrganizationSetupState>()
            .Where(x => x.OrganizationId == organizationId)
            .Build(), ct);

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
        if (!setup.AccountingCompleted)
            return "accounting-policy";
        if (!setup.DefaultsCompleted)
            return "defaults";
        return "complete";
    }

    private static string? GetMissingStep(OrganizationSetupState setup)
    {
        if (!setup.OrganizationCompleted)
            return "company-profile";
        if (!setup.AccountingCompleted)
            return "accounting-policy";
        if (!setup.DefaultsCompleted)
            return "defaults";
        return null;
    }
}
