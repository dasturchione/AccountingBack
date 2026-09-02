using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Departments;
using Domain.Entities;
using Infrastructure.Repositories;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.Org.Departments;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class DepartmentQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 41001;
    private const int OrganizationId = 41001;
    private const int OtherOrganizationId = 41002;
    private const int RegionId = 41001;
    private const int BranchId = 41101;

    [Fact]
    public async Task ListAndDetailStayInsideSelectedOrganizationForSuperAdmin()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IDepartmentService>();

        var page = await service.GetAllAsync(new DepartmentListFilter
        {
            Search = "scoped department",
            Page = 2,
            PageSize = 1
        });
        var byBranch = await service.GetAllAsync(new DepartmentListFilter
        {
            BranchId = BranchId,
            Page = 1,
            PageSize = 10
        });
        var ownDetail = await service.GetByIdAsync(41201);
        var otherDetail = await service.GetByIdAsync(41203);

        Assert.True(page.IsSuccess);
        Assert.Equal(2, page.Value.TotalCount);
        Assert.Equal("Beta scoped department", Assert.Single(page.Value.Items).Name);
        Assert.True(byBranch.IsSuccess);
        Assert.Equal(2, byBranch.Value.TotalCount);
        Assert.All(byBranch.Value.Items, item => Assert.Equal(BranchId, item.BranchId));
        Assert.True(ownDetail.IsSuccess);
        Assert.Equal("Selected department branch", ownDetail.Value.BranchName);
        Assert.Equal("Active", ownDetail.Value.StateName);
        Assert.False(otherDetail.IsSuccess);
        Assert.Equal("Department.NotFound", otherDetail.Error.Code);
        Assert.Equal("Отдел с id 41203 не найден.", otherDetail.Error.Description);
    }

    [Fact]
    public async Task CodeUniquenessIsOrganizationLocalAndMissingContextReturnsResultError()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IDepartmentService>();

        var crossOrganizationBranch = await service.CreateAsync(new DepartmentCreateDto
        {
            BranchId = 41102,
            Code = "INVALID-CROSS-BRANCH",
            Name = "Invalid cross-organization branch"
        });
        var created = await service.CreateAsync(new DepartmentCreateDto
        {
            BranchId = BranchId,
            Code = "CROSS-DEPARTMENT-CODE",
            Name = "Created department"
        });

        Assert.False(crossOrganizationBranch.IsSuccess);
        Assert.Equal("Department.BranchNotFound", crossOrganizationBranch.Error.Code);
        Assert.True(created.IsSuccess);
        await using (var context = fixture.CreateDbContext())
        {
            Assert.True(await context.Departments.IgnoreQueryFilters().AnyAsync(department =>
                department.Id == created.Value
                && department.OrganizationId == OrganizationId
                && department.Code == "CROSS-DEPARTMENT-CODE"));
        }

        await using var missingProvider = CreateProvider(User(null));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missingContext = await missingScope.ServiceProvider
            .GetRequiredService<IDepartmentService>()
            .GetAllAsync(new DepartmentListFilter());

        Assert.False(missingContext.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missingContext.Error.Code);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services =>
            {
                services.AddLogging();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<IDepartmentService, DepartmentService>();
            });

    private static IntegrationTestUserContext User(int? organizationId) => new()
    {
        Id = 41001,
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
                ShortName = "Department region",
                FullName = "Department region",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = TenantId,
                Name = "Department tenant",
                Slug = "department-query-contract-tenant",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Selected department organization", "410000001"),
                Organization(OtherOrganizationId, "Other department organization", "410000002"));
        }

        if (!await context.Branches.IgnoreQueryFilters().AnyAsync(branch => branch.Id == BranchId))
        {
            context.Branches.AddRange(
                Branch(BranchId, OrganizationId, "Selected department branch"),
                Branch(41102, OtherOrganizationId, "Other department branch"));
        }

        if (!await context.Departments.IgnoreQueryFilters().AnyAsync(department => department.Id == 41201))
        {
            context.Departments.AddRange(
                Department(41201, OrganizationId, BranchId, "OWN-DEP-A", "Alpha scoped department"),
                Department(41202, OrganizationId, BranchId, "OWN-DEP-B", "Beta scoped department"),
                Department(41203, OtherOrganizationId, 41102, "CROSS-DEPARTMENT-CODE", "Other scoped department"));
        }

        await context.Departments.IgnoreQueryFilters()
            .Where(department => department.OrganizationId == OrganizationId && department.Code == "CROSS-DEPARTMENT-CODE")
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

    private static Branch Branch(int id, int organizationId, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        Code = $"BR-{id}",
        Name = name,
        RegionId = RegionId,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static Department Department(int id, int organizationId, int branchId, string code, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        BranchId = branchId,
        Code = code,
        Name = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };
}
