using Application.Abstractions.Authentication;
using Application.Features.Branches;
using Domain.Entities;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.Org.Branches;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class BranchQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 40001;
    private const int OrganizationId = 40001;
    private const int OtherOrganizationId = 40002;
    private const int RegionId = 40001;

    [Fact]
    public async Task ListAndDetailStayInsideSelectedOrganizationForSuperAdmin()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBranchService>();

        var page = await service.GetAllAsync(new BranchListFilter
        {
            RegionId = RegionId,
            Search = "scoped branch",
            Page = 2,
            PageSize = 1
        });
        var ownDetail = await service.GetByIdAsync(40101);
        var otherDetail = await service.GetByIdAsync(40103);

        Assert.True(page.IsSuccess);
        Assert.Equal(2, page.Value.TotalCount);
        Assert.Equal("Beta scoped branch", Assert.Single(page.Value.Items).Name);
        Assert.True(ownDetail.IsSuccess);
        Assert.Equal(OrganizationId, ownDetail.Value.OrganizationId);
        Assert.Equal("Branch region", ownDetail.Value.RegionName);
        Assert.Equal("Active", ownDetail.Value.StateName);
        Assert.False(otherDetail.IsSuccess);
        Assert.Equal("Branch.NotFound", otherDetail.Error.Code);
        Assert.Equal("Филиал с id 40103 не найден.", otherDetail.Error.Description);
    }

    [Fact]
    public async Task CodeUniquenessIsOrganizationLocalAndMissingContextReturnsResultError()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBranchService>();

        var created = await service.CreateAsync(new BranchCreateDto
        {
            Code = "CROSS-ORG-CODE",
            Name = "Created in selected organization",
            RegionId = RegionId
        });

        Assert.True(created.IsSuccess);
        await using (var context = fixture.CreateDbContext())
        {
            Assert.True(await context.Branches.IgnoreQueryFilters().AnyAsync(branch =>
                branch.Id == created.Value
                && branch.OrganizationId == OrganizationId
                && branch.Code == "CROSS-ORG-CODE"));
        }

        await using var missingProvider = CreateProvider(User(null));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missingContext = await missingScope.ServiceProvider
            .GetRequiredService<IBranchService>()
            .CreateAsync(new BranchCreateDto { Code = "NO-ORG", Name = "No organization" });

        Assert.False(missingContext.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missingContext.Error.Code);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<IBranchService, BranchService>());

    private static IntegrationTestUserContext User(int? organizationId) => new()
    {
        Id = 40001,
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
                ShortName = "Branch region",
                FullName = "Branch region",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = TenantId,
                Name = "Branch tenant",
                Slug = "branch-query-contract-tenant",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Selected branch organization", "400000001"),
                Organization(OtherOrganizationId, "Other branch organization", "400000002"));
        }

        if (!await context.Branches.IgnoreQueryFilters().AnyAsync(branch => branch.Id == 40101))
        {
            context.Branches.AddRange(
                Branch(40101, OrganizationId, "OWN-A", "Alpha scoped branch"),
                Branch(40102, OrganizationId, "OWN-B", "Beta scoped branch"),
                Branch(40103, OtherOrganizationId, "CROSS-ORG-CODE", "Other scoped branch"));
        }

        await context.Branches.IgnoreQueryFilters()
            .Where(branch => branch.OrganizationId == OrganizationId && branch.Code == "CROSS-ORG-CODE")
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
}
