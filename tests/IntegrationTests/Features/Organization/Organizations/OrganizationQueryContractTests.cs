using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration;
using Application.Abstractions.Integration.Models;
using Application.Features.Organizations;
using Domain.Entities;
using Infrastructure.Repositories;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.OrganizationManagement.Organizations;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class OrganizationQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 43001;
    private const int OtherTenantId = 43002;
    private const int OrganizationId = 43001;
    private const int SecondOrganizationId = 43002;
    private const int OtherOrganizationId = 43003;
    private const int RegionId = 43001;
    private const int OtherRegionId = 43002;

    [Fact]
    public async Task ListPreservesUserKindVisibilityAndSqlFilterOrderPaging()
    {
        await SeedAsync();

        var superAdmin = await GetPageAsync(User(CurrentUserKind.SuperAdmin, null, null, []), new OrganizationListFilter
        {
            RegionId = RegionId,
            IsParent = false,
            Search = "query organization",
            Page = 2,
            PageSize = 1
        });
        var tenantAdmin = await GetPageAsync(User(CurrentUserKind.TenantAdmin, TenantId, null, []), new OrganizationListFilter
        {
            Page = 1,
            PageSize = 10
        });
        var tenantUser = await GetPageAsync(User(
            CurrentUserKind.TenantUser,
            TenantId,
            OrganizationId,
            [OrganizationId]), new OrganizationListFilter
        {
            Page = 1,
            PageSize = 10
        });

        Assert.True(superAdmin.IsSuccess);
        Assert.Equal(2, superAdmin.Value.TotalCount);
        Assert.Equal("Beta query organization", Assert.Single(superAdmin.Value.Items).ShortName);
        Assert.Equal([OrganizationId, SecondOrganizationId], OwnedIds(tenantAdmin.Value.Items));
        Assert.Equal([OrganizationId], OwnedIds(tenantUser.Value.Items));
    }

    [Fact]
    public async Task DetailUsesBaseReferenceNamesAndHonorsOrganizationVisibility()
    {
        await SeedAsync();
        var user = User(CurrentUserKind.TenantUser, TenantId, OrganizationId, [OrganizationId]);
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IOrganizationService>();

        var own = await service.GetByIdAsync(OrganizationId);
        var forbidden = await service.GetByIdAsync(OtherOrganizationId);

        Assert.True(own.IsSuccess);
        Assert.Equal("Organization region", own.Value.RegionName);
        Assert.Equal("Active", own.Value.StateName);
        Assert.Equal("Russian", own.Value.DefaultLanguageName);
        Assert.Equal("Organization address", own.Value.Address);
        Assert.False(forbidden.IsSuccess);
        Assert.Equal("Organization.NotFound", forbidden.Error.Code);
        Assert.Equal($"Организация с id {OtherOrganizationId} не найдена.", forbidden.Error.Description);
    }

    [Fact]
    public async Task InnLookupReturnsExternalProviderContract()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(CurrentUserKind.SuperAdmin, null, null, []));
        await using var scope = provider.CreateAsyncScope();
        var result = await scope.ServiceProvider
            .GetRequiredService<IOrganizationService>()
            .GetByInnAsync("430000001");

        Assert.True(result.IsSuccess);
        Assert.Equal("430000001", result.Value.CompanyInn);
        Assert.Equal("External Faktura company", result.Value.CompanyName);
    }

    private async Task<SharedKernel.Results.Result<Application.Common.Pagination.PagedResponse<OrganizationListDto>>> GetPageAsync(
        IntegrationTestUserContext user,
        OrganizationListFilter filter)
    {
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IOrganizationService>().GetAllAsync(filter);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services =>
            {
                services.AddLogging();
                services.AddSingleton<IFakturaService, FakeFakturaService>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<IOrganizationManagementCore, OrganizationManagementCore>();
                services.AddScoped<IOrganizationService, OrganizationService>();
            });

    private static IntegrationTestUserContext User(
        CurrentUserKind kind,
        int? tenantId,
        int? organizationId,
        List<int> allowedOrganizationIds) => new()
    {
        Id = 43001,
        UserKind = kind,
        LanguageId = LanguageIdConst.RU,
        TenantId = tenantId,
        OrganizationId = organizationId,
        AllowedOrganizationIds = allowedOrganizationIds
    };

    private static int[] OwnedIds(IEnumerable<OrganizationListDto> items) => items
        .Where(item => item.Id is OrganizationId or SecondOrganizationId or OtherOrganizationId)
        .Select(item => item.Id)
        .Order()
        .ToArray();

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

        if (!await context.Languages.AnyAsync(language => language.Id == LanguageIdConst.RU))
        {
            context.Languages.Add(new Language
            {
                Id = LanguageIdConst.RU,
                Code = "ru",
                Name = "Russian",
                NativeName = "Русский",
                IsDefault = false,
                SortOrder = 2,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Regions.AnyAsync(region => region.Id == RegionId))
        {
            context.Regions.AddRange(
                Region(RegionId, "Organization region"),
                Region(OtherRegionId, "Other organization region"));
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.AddRange(
                Tenant(TenantId, "organization-query-tenant"),
                Tenant(OtherTenantId, "other-organization-query-tenant"));
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                CreateOrganization(OrganizationId, TenantId, RegionId, "Alpha query organization", "430000001", false),
                CreateOrganization(SecondOrganizationId, TenantId, RegionId, "Beta query organization", "430000002", false),
                CreateOrganization(OtherOrganizationId, OtherTenantId, OtherRegionId, "Other tenant organization", "430000003", true));
        }

        await context.SaveChangesAsync();
    }

    private static Region Region(int id, string name) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static PlatformTenant Tenant(int id, string slug) => new()
    {
        Id = id,
        Name = slug,
        Slug = slug,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static Organization CreateOrganization(
        int id,
        int tenantId,
        int regionId,
        string name,
        string inn,
        bool isParent) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = inn,
        RegionId = regionId,
        Address = id == OrganizationId ? "Organization address" : null,
        IsParent = isParent,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate,
        DefaultLanguageId = id == OrganizationId ? LanguageIdConst.RU : null,
        TenantId = tenantId,
        SetupStatus = "completed"
    };

    private sealed class FakeFakturaService : IFakturaService
    {
        public Task<CompanyBasicDetailsDto> GetCompanyDataAsync(string companyInn) =>
            Task.FromResult(new CompanyBasicDetailsDto
            {
                CompanyInn = companyInn,
                CompanyName = "External Faktura company"
            });
    }
}
