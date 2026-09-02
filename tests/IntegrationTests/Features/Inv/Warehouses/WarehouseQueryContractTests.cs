using Application.Abstractions.Authentication;
using Application.Features.Warehouses;
using Domain.Entities;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.Inv.Warehouses;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class WarehouseQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 50001;
    private const int OrganizationId = 50001;
    private const int OtherOrganizationId = 50002;
    private const int RegionId = 50001;
    private const int BranchId = 50001;
    private const int OtherBranchId = 50002;

    [Fact]
    public async Task ListAndDetailStayInsideSelectedOrganizationForSuperAdmin()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IWarehouseService>();

        var page = await service.GetAllAsync(new WarehouseListFilter
        {
            BranchId = BranchId,
            Search = "scoped warehouse",
            Page = 2,
            PageSize = 1
        });
        var own = await service.GetByIdAsync(50101);
        var foreign = await service.GetByIdAsync(50103);

        Assert.True(page.IsSuccess);
        Assert.Equal(2, page.Value.TotalCount);
        Assert.Equal("Beta scoped warehouse", Assert.Single(page.Value.Items).Name);
        Assert.True(own.IsSuccess);
        Assert.Equal(OrganizationId, own.Value.OrganizationId);
        Assert.Equal("Warehouse branch", own.Value.BranchName);
        Assert.Equal("Active", own.Value.StateName);
        Assert.False(foreign.IsSuccess);
        Assert.Equal("Warehouse.NotFound", foreign.Error.Code);
    }

    [Fact]
    public async Task CodeIsOrganizationLocalAndReferencesMustBelongToOrganization()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IWarehouseService>();

        var created = await service.CreateAsync(CreateDto("CROSS-ORG-WAREHOUSE", BranchId));
        var duplicate = await service.CreateAsync(CreateDto("OWN-WAREHOUSE", BranchId));
        var foreignBranch = await service.CreateAsync(CreateDto("FOREIGN-BRANCH-WAREHOUSE", OtherBranchId));
        var unknownResponsibleUser = await service.CreateAsync(new WarehouseCreateDto
        {
            Code = "UNKNOWN-RESPONSIBLE",
            Name = "Unknown responsible user",
            BranchId = BranchId,
            ResponsibleUserId = 59999
        });

        Assert.True(created.IsSuccess);
        Assert.False(duplicate.IsSuccess);
        Assert.Equal("Warehouse.CodeConflict", duplicate.Error.Code);
        Assert.False(foreignBranch.IsSuccess);
        Assert.Equal("Warehouse.BranchNotFound", foreignBranch.Error.Code);
        Assert.False(unknownResponsibleUser.IsSuccess);
        Assert.Equal("Warehouse.ResponsibleUserNotFound", unknownResponsibleUser.Error.Code);
    }

    [Fact]
    public async Task MissingOrganizationAndForeignUpdateReturnResultErrors()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var foreignUpdate = await scope.ServiceProvider
            .GetRequiredService<IWarehouseService>()
            .UpdateAsync(50103, new WarehouseUpdateDto
            {
                Code = "FOREIGN-UPDATED",
                Name = "Foreign updated",
                StateId = StateIdConst.ACTIVE
            });

        Assert.False(foreignUpdate.IsSuccess);
        Assert.Equal("Warehouse.NotFound", foreignUpdate.Error.Code);

        await using var missingProvider = CreateProvider(User(null));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missing = await missingScope.ServiceProvider
            .GetRequiredService<IWarehouseService>()
            .GetAllAsync(new WarehouseListFilter());

        Assert.False(missing.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missing.Error.Code);
    }

    private static WarehouseCreateDto CreateDto(string code, int branchId) => new()
    {
        Code = code,
        Name = $"Warehouse {code}",
        BranchId = branchId
    };

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<IWarehouseService, WarehouseService>());

    private static IntegrationTestUserContext User(int? organizationId) => new()
    {
        Id = 50001,
        UserKind = CurrentUserKind.SuperAdmin,
        LanguageId = LanguageIdConst.RU,
        TenantId = TenantId,
        OrganizationId = organizationId,
        AllowedOrganizationIds = organizationId.HasValue ? [organizationId.Value] : []
    };

    private async Task SeedAsync()
    {
        await using var context = fixture.CreateDbContext();

        if (!await context.States.AnyAsync(state => state.Id == StateIdConst.ACTIVE))
        {
            context.States.Add(new State
            {
                Id = StateIdConst.ACTIVE,
                ShortName = "Active",
                FullName = "Active",
                CreatedDate = SeedDate
            });
        }

        if (!await context.Regions.AnyAsync(region => region.Id == RegionId))
        {
            context.Regions.Add(new Region
            {
                Id = RegionId,
                ShortName = "Warehouse region",
                FullName = "Warehouse region",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = TenantId,
                Name = "Warehouse tenant",
                Slug = "warehouse-contract-tenant",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Selected warehouse organization", "500000001"),
                Organization(OtherOrganizationId, "Other warehouse organization", "500000002"));
        }

        if (!await context.Branches.IgnoreQueryFilters().AnyAsync(branch => branch.Id == BranchId))
        {
            context.Branches.AddRange(
                Branch(BranchId, OrganizationId, "WAREHOUSE-BRANCH", "Warehouse branch"),
                Branch(OtherBranchId, OtherOrganizationId, "OTHER-WAREHOUSE-BRANCH", "Other warehouse branch"));
        }

        if (!await context.Warehouses.IgnoreQueryFilters().AnyAsync(warehouse => warehouse.Id == 50101))
        {
            context.Warehouses.AddRange(
                Warehouse(50101, OrganizationId, BranchId, "OWN-WAREHOUSE", "Alpha scoped warehouse", true),
                Warehouse(50102, OrganizationId, BranchId, "OWN-WAREHOUSE-B", "Beta scoped warehouse", false),
                Warehouse(50103, OtherOrganizationId, OtherBranchId, "CROSS-ORG-WAREHOUSE", "Other scoped warehouse", true));
        }

        await context.Warehouses.IgnoreQueryFilters()
            .Where(warehouse => warehouse.OrganizationId == OrganizationId &&
                                (warehouse.Code == "CROSS-ORG-WAREHOUSE" ||
                                 warehouse.Code == "FOREIGN-BRANCH-WAREHOUSE" ||
                                 warehouse.Code == "UNKNOWN-RESPONSIBLE"))
            .ExecuteDeleteAsync();

        await context.SaveChangesAsync();
    }

    private static Organization Organization(int id, string name, string inn) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = inn,
        RegionId = RegionId,
        IsParent = false,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate,
        TenantId = TenantId,
        SetupStatus = "completed"
    };

    private static Branch Branch(int id, int organizationId, string code, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        Code = code,
        Name = name,
        RegionId = RegionId,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static Warehouse Warehouse(
        int id,
        int organizationId,
        int branchId,
        string code,
        string name,
        bool isMain) => new()
        {
            Id = id,
            OrganizationId = organizationId,
            BranchId = branchId,
            Code = code,
            Name = name,
            IsMain = isMain,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = SeedDate
        };
}
