using Application.Features.OrganizationSetup;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class OrganizationSetupCoreTests
{
    [Fact]
    public async Task SeedWorkspaceSetupAsync_ShouldCreateCompletedBootstrapSnapshot()
    {
        var setups = new List<OrganizationSetupState>();
        var taxes = new List<OrganizationTaxSetting>();
        var configs = new List<OrganizationConfig>();
        var defaults = new List<OrganizationDefault>();
        var organizations = new List<Organization>
        {
            CreateOrganization(810)
        };
        var core = CreateCore(setups, taxes, configs, defaults, organizations);
        var now = new DateTime(2026, 7, 5, 12, 0, 0);

        await core.SeedWorkspaceSetupAsync(
            new WorkspaceSetupInitializationRequest
            {
                OrganizationId = 810,
                HasTax = true,
                TaxSettings = new OrganizationSetupTaxSettingsWriteModel
                {
                    TaxTypeId = 1,
                    IsVatPayer = true,
                    VatRegistrationNumber = "VAT-810",
                    EffectiveFrom = new DateOnly(2026, 1, 1),
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = now
                },
                HasAccounting = true,
                AccountingPolicy = new OrganizationSetupAccountingPolicyWriteModel
                {
                    InventoryValuationMethod = "fifo",
                    AccountingPolicyId = 1,
                    BaseCurrencyId = 1,
                    AccountingStartDate = new DateOnly(2026, 1, 1),
                    FiscalYearStartMonth = 3
                },
                HasDefaults = true,
                Defaults = new OrganizationSetupDefaultsWriteModel
                {
                    BranchId = 101,
                    WarehouseId = 102,
                    CogsAccountId = 112,
                    CreatedDate = now
                },
                UsersCompleted = true,
                IsCompleted = true,
                Timestamp = now
            });

        Assert.Single(taxes);
        Assert.Single(configs);
        Assert.Single(defaults);
        Assert.Single(setups);
        Assert.Equal("complete", setups.Single().CurrentStep);
        Assert.True(setups.Single().OrganizationCompleted);
        Assert.True(setups.Single().TaxCompleted);
        Assert.True(setups.Single().AccountingCompleted);
        Assert.True(setups.Single().DefaultsCompleted);
        Assert.True(setups.Single().UsersCompleted);
        Assert.True(setups.Single().IsCompleted);
        Assert.Equal(now, setups.Single().CompletedAt);
        Assert.Equal("fifo", configs.Single().InventoryValuationMethod);
        Assert.Equal(101, defaults.Single().BranchId);
    }

    [Fact]
    public async Task UpdateSetupStateAsync_ShouldAdvanceCurrentStep_WithoutAutoCompleting()
    {
        var setups = new List<OrganizationSetupState>();
        var core = CreateCore(
            setups,
            [],
            [],
            [],
            [CreateOrganization(820)]);

        await core.UpdateSetupStateAsync(820, setup => setup.OrganizationCompleted = true);
        await core.UpdateSetupStateAsync(820, setup => setup.TaxCompleted = true);
        await core.UpdateSetupStateAsync(820, setup => setup.AccountingCompleted = true);

        var setupState = setups.Single();
        Assert.False(setupState.IsCompleted);
        Assert.Equal("defaults", setupState.CurrentStep);
        Assert.True(setupState.OrganizationCompleted);
        Assert.True(setupState.TaxCompleted);
        Assert.True(setupState.AccountingCompleted);
        Assert.False(setupState.DefaultsCompleted);
    }

    [Fact]
    public async Task CompleteSetupAsync_ShouldMarkOrganizationAndSetupCompleted()
    {
        var organization = CreateOrganization(830);
        var organizations = new List<Organization> { organization };
        var setups = new List<OrganizationSetupState>
        {
            new()
            {
                OrganizationId = 830,
                CurrentStep = "users",
                OrganizationCompleted = true,
                TaxCompleted = true,
                AccountingCompleted = true,
                DefaultsCompleted = true,
                UsersCompleted = true,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            }
        };
        var core = CreateCore(setups, [], [], [], organizations);

        var result = await core.CompleteSetupAsync(organization);

        Assert.True(result.IsSuccess);
        Assert.True(setups.Single().IsCompleted);
        Assert.Equal("complete", setups.Single().CurrentStep);
        Assert.Equal("completed", organization.SetupStatus);
        Assert.NotNull(organization.SetupCompletedAt);
    }

    private static OrganizationSetupCore CreateCore(
        List<OrganizationSetupState> setups,
        List<OrganizationTaxSetting> taxes,
        List<OrganizationConfig> configs,
        List<OrganizationDefault> defaults,
        List<Organization> organizations) =>
        new(
            new InMemoryQueryRepository<OrganizationSetupState>(setups),
            new InMemoryCommandRepository<OrganizationSetupState>(setups),
            new InMemoryQueryRepository<OrganizationTaxSetting>(taxes),
            new InMemoryCommandRepository<OrganizationTaxSetting>(taxes),
            new InMemoryQueryRepository<OrganizationConfig>(configs),
            new InMemoryCommandRepository<OrganizationConfig>(configs),
            new InMemoryQueryRepository<OrganizationDefault>(defaults),
            new InMemoryCommandRepository<OrganizationDefault>(defaults),
            new InMemoryCommandRepository<Organization>(organizations));

    private static Organization CreateOrganization(int id) =>
        new()
        {
            Id = id,
            ShortName = $"ORG-{id}",
            FullName = $"Organization {id}",
            Inn = $"{id}{id}{id}",
            RegionId = 1,
            IsParent = true,
            SetupStatus = "pending",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.UtcNow
        };
}
