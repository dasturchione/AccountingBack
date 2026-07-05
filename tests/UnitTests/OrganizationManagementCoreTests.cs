using Application.Features.Organizations;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class OrganizationManagementCoreTests
{
    [Fact]
    public async Task GetOrganizationAsync_GlobalScope_ShouldRequireGlobalAccess()
    {
        var core = CreateCore(
            new FakeUserContext { HasGlobalAccess = false },
            [
                CreateOrganization(710, "710710710")
            ],
            []);

        var result = await core.GetOrganizationAsync(
            710,
            OrganizationManagementOptions.ForGlobal());

        Assert.False(result.IsSuccess);
        Assert.Equal("Platform.GlobalAccessRequired", result.Error.Code);
    }

    [Fact]
    public async Task UpdateOrganizationAsync_OrganizationScope_ShouldAllowUnknownTenant_AndKeepRawFields()
    {
        var organizations = new List<Organization>
        {
            CreateOrganization(711, "711711711", setupStatus: "completed", tenantId: 1)
        };
        var core = CreateCore(
            new FakeUserContext { HasGlobalAccess = false, LanguageId = LanguageIdConst.EN },
            organizations,
            []);

        var result = await core.UpdateOrganizationAsync(
            new OrganizationManagementUpdateRequest
            {
                OrganizationId = 711,
                ShortName = " ORG-711-NEW ",
                FullName = " Organization 711 Updated ",
                Inn = " 711711711 ",
                PhoneNumber = "998901117117",
                RegionId = 2,
                DistrictId = 20,
                Address = "Address 711",
                Director = "Director 711",
                IsParent = false,
                DefaultLanguageId = LanguageIdConst.RU,
                TenantId = 999,
                SetupStatus = "",
                SetupCompletedAt = new DateTime(2026, 2, 1),
                Email = "org711@example.com",
                Website = "https://org711.example.com",
                Oked = "71111",
                StateId = StateIdConst.PASSIVE
            },
            OrganizationManagementOptions.ForOrganization());

        Assert.True(result.IsSuccess);

        var organization = organizations.Single();
        Assert.Equal(" ORG-711-NEW ", organization.ShortName);
        Assert.Equal(" Organization 711 Updated ", organization.FullName);
        Assert.Equal(" 711711711 ", organization.Inn);
        Assert.Equal(999, organization.TenantId);
        Assert.Equal("completed", organization.SetupStatus);
        Assert.Equal(new DateTime(2026, 2, 1), organization.SetupCompletedAt);
        Assert.Equal(StateIdConst.PASSIVE, organization.StateId);
    }

    [Fact]
    public async Task UpdateOrganizationAsync_GlobalScope_ShouldTrimFields_AndPreserveSetupStatusWhenBlank()
    {
        var organizations = new List<Organization>
        {
            CreateOrganization(720, "720720720", setupStatus: "pending", tenantId: 801)
        };
        var tenants = new List<PlatformTenant>
        {
            new()
            {
                Id = 802,
                Name = "Tenant 802",
                Slug = "tenant-802",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            }
        };
        var core = CreateCore(
            new FakeUserContext { HasGlobalAccess = true },
            organizations,
            tenants);

        var result = await core.UpdateOrganizationAsync(
            new OrganizationManagementUpdateRequest
            {
                OrganizationId = 720,
                ShortName = " ORG-720-NEW ",
                FullName = " Organization 720 Updated ",
                Inn = " 720720720 ",
                PhoneNumber = "998901234720",
                RegionId = 3,
                DistrictId = 30,
                Address = "Platform address",
                Director = "Platform director",
                IsParent = true,
                DefaultLanguageId = LanguageIdConst.RU,
                TenantId = 802,
                SetupStatus = "",
                SetupCompletedAt = new DateTime(2026, 3, 1),
                Email = "new-platform@example.com",
                Website = "https://new-platform.example.com",
                Oked = "54321",
                StateId = StateIdConst.PASSIVE
            },
            OrganizationManagementOptions.ForGlobal());

        Assert.True(result.IsSuccess);

        var organization = organizations.Single();
        Assert.Equal("ORG-720-NEW", organization.ShortName);
        Assert.Equal("Organization 720 Updated", organization.FullName);
        Assert.Equal("720720720", organization.Inn);
        Assert.Equal(802, organization.TenantId);
        Assert.Equal("pending", organization.SetupStatus);
        Assert.Equal(new DateTime(2026, 3, 1), organization.SetupCompletedAt);
        Assert.Equal(StateIdConst.PASSIVE, organization.StateId);
    }

    private static OrganizationManagementCore CreateCore(
        FakeUserContext userContext,
        List<Organization> organizations,
        List<PlatformTenant> tenants) =>
        new(
            userContext,
            new InMemoryQueryRepository<Organization>(organizations),
            new InMemoryCommandRepository<Organization>(organizations),
            new InMemoryQueryRepository<PlatformTenant>(tenants));

    private static Organization CreateOrganization(
        int id,
        string inn,
        string setupStatus = "pending",
        int? tenantId = null) =>
        new()
        {
            Id = id,
            ShortName = $"ORG-{id}",
            FullName = $"Organization {id}",
            Inn = inn,
            RegionId = 1,
            DistrictId = 10,
            Address = "Old address",
            Director = "Old director",
            IsParent = true,
            DefaultLanguageId = LanguageIdConst.UZ,
            TenantId = tenantId,
            SetupStatus = setupStatus,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.UtcNow,
            Region = new Region { Id = 1, ShortName = "R1", FullName = "Region 1", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.UtcNow },
            District = new District { Id = 10, ShortName = "D10", FullName = "District 10", RegionId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.UtcNow },
            State = new State { Id = StateIdConst.ACTIVE, ShortName = "ACTIVE", FullName = "Active", CreatedDate = DateTime.UtcNow },
            DefaultLanguage = new Language { Id = LanguageIdConst.UZ, Code = "uz", Name = "Uzbek", NativeName = "O'zbek", IsDefault = true, SortOrder = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.UtcNow }
        };
}
