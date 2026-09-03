using Application.Abstractions.Authentication;
using Application.Features.Positions;
using Domain.Entities;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.Org.Positions;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class PositionQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 42001;
    private const int OrganizationId = 42001;
    private const int OtherOrganizationId = 42002;
    private const int RegionId = 42001;

    [Fact]
    public async Task ListAndDetailStayInsideSelectedOrganizationForSuperAdmin()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IPositionService>();

        var page = await service.GetAllAsync(new PositionListFilter
        {
            Search = "scoped position",
            Page = 2,
            PageSize = 1
        });
        var ownDetail = await service.GetByIdAsync(42101);
        var otherDetail = await service.GetByIdAsync(42103);

        Assert.True(page.IsSuccess);
        Assert.Equal(2, page.Value.TotalCount);
        Assert.Equal("Beta scoped position", Assert.Single(page.Value.Items).Name);
        Assert.True(ownDetail.IsSuccess);
        Assert.Equal(OrganizationId, ownDetail.Value.OrganizationId);
        Assert.Equal("Active", ownDetail.Value.StateName);
        Assert.False(otherDetail.IsSuccess);
        Assert.Equal("Position.NotFound", otherDetail.Error.Code);
        Assert.Equal("Должность с id 42103 не найдена.", otherDetail.Error.Description);
    }

    [Fact]
    public async Task CodeUniquenessIsOrganizationLocalAndMissingContextReturnsResultError()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IPositionService>();

        var created = await service.CreateAsync(new PositionCreateDto
        {
            Code = "CROSS-POSITION-CODE",
            Name = "Created position"
        });

        Assert.True(created.IsSuccess);
        await using (var context = fixture.CreateDbContext())
        {
            Assert.True(await context.Positions.IgnoreQueryFilters().AnyAsync(position =>
                position.Id == created.Value
                && position.OrganizationId == OrganizationId
                && position.Code == "CROSS-POSITION-CODE"));
        }

        await using var missingProvider = CreateProvider(User(null));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missingContext = await missingScope.ServiceProvider
            .GetRequiredService<IPositionService>()
            .GetAllAsync(new PositionListFilter());

        Assert.False(missingContext.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missingContext.Error.Code);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<IPositionService, PositionService>());

    private static IntegrationTestUserContext User(int? organizationId) => new()
    {
        Id = 42001,
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
                ShortName = "Position region",
                FullName = "Position region",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = TenantId,
                Name = "Position tenant",
                Slug = "position-query-contract-tenant",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Selected position organization", "420000001"),
                Organization(OtherOrganizationId, "Other position organization", "420000002"));
        }

        if (!await context.Positions.IgnoreQueryFilters().AnyAsync(position => position.Id == 42101))
        {
            context.Positions.AddRange(
                Position(42101, OrganizationId, "OWN-POS-A", "Alpha scoped position"),
                Position(42102, OrganizationId, "OWN-POS-B", "Beta scoped position"),
                Position(42103, OtherOrganizationId, "CROSS-POSITION-CODE", "Other scoped position"));
        }

        await context.Positions.IgnoreQueryFilters()
            .Where(position => position.OrganizationId == OrganizationId && position.Code == "CROSS-POSITION-CODE")
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

    private static Position Position(int id, int organizationId, string code, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        Code = code,
        Name = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };
}
