using Application.Abstractions.Authentication;
using Application.Features.CounterpartyCards;
using Domain.Entities;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.CounterpartyManagement.CounterpartyCards;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class CounterpartyCardQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 45001;
    private const int OrganizationId = 45001;
    private const int OtherOrganizationId = 45002;
    private const int RegionId = 45001;
    private const int DistrictId = 45001;

    [Fact]
    public async Task ListAndDetailStayInsideSelectedOrganizationForSuperAdmin()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICounterpartyCardService>();

        var page = await service.GetAllAsync(new CounterpartyCardListFilter
        {
            Search = "scoped counterparty",
            Page = 2,
            PageSize = 1
        });
        var ownDetail = await service.GetByIdAsync(45101);
        var otherDetail = await service.GetByIdAsync(45103);

        Assert.True(page.IsSuccess);
        Assert.Equal(2, page.Value.TotalCount);
        Assert.Equal("Beta scoped counterparty", Assert.Single(page.Value.Items).ShortName);
        Assert.True(ownDetail.IsSuccess);
        Assert.Equal(OrganizationId, ownDetail.Value.OrganizationId);
        Assert.Equal("Counterparty region", ownDetail.Value.RegionName);
        Assert.Equal("Counterparty district", ownDetail.Value.DistrictName);
        Assert.Equal("Active", ownDetail.Value.StateName);
        Assert.False(otherDetail.IsSuccess);
        Assert.Equal("CounterpartyCard.NotFound", otherDetail.Error.Code);
        Assert.Equal("Контрагент с id 45103 не найден.", otherDetail.Error.Description);
    }

    [Fact]
    public async Task ShortNameUniquenessIsOrganizationLocalAndMissingContextReturnsResultError()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICounterpartyCardService>();

        var created = await service.CreateAsync(new CounterpartyCardCreateDto
        {
            ShortName = "CROSS-ORG-NAME",
            RegionId = RegionId,
            DistrictId = DistrictId
        });

        Assert.True(created.IsSuccess);
        await using (var context = fixture.CreateDbContext())
        {
            Assert.True(await context.CounterpartyCards.IgnoreQueryFilters().AnyAsync(counterparty =>
                counterparty.Id == created.Value.Id
                && counterparty.OrganizationId == OrganizationId
                && counterparty.ShortName == "CROSS-ORG-NAME"));
        }

        await using var missingProvider = CreateProvider(User(null));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missingContext = await missingScope.ServiceProvider
            .GetRequiredService<ICounterpartyCardService>()
            .CreateAsync(new CounterpartyCardCreateDto { ShortName = "NO-ORG" });

        Assert.False(missingContext.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missingContext.Error.Code);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<ICounterpartyCardService, CounterpartyCardService>());

    private static IntegrationTestUserContext User(int? organizationId) => new()
    {
        Id = 45001,
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
                ShortName = "Counterparty region",
                FullName = "Counterparty region",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Districts.AnyAsync(district => district.Id == DistrictId))
        {
            context.Districts.Add(new District
            {
                Id = DistrictId,
                ShortName = "Counterparty district",
                FullName = "Counterparty district",
                RegionId = RegionId,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = TenantId,
                Name = "Counterparty tenant",
                Slug = "counterparty-query-contract-tenant",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Selected counterparty organization", "450000001"),
                Organization(OtherOrganizationId, "Other counterparty organization", "450000002"));
        }

        if (!await context.CounterpartyCards.IgnoreQueryFilters().AnyAsync(counterparty => counterparty.Id == 45101))
        {
            context.CounterpartyCards.AddRange(
                Counterparty(45101, OrganizationId, "Alpha scoped counterparty"),
                Counterparty(45102, OrganizationId, "Beta scoped counterparty"),
                Counterparty(45103, OtherOrganizationId, "CROSS-ORG-NAME"));
        }

        await context.CounterpartyCards.IgnoreQueryFilters()
            .Where(counterparty => counterparty.OrganizationId == OrganizationId && counterparty.ShortName == "CROSS-ORG-NAME")
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

    private static CounterpartyCard Counterparty(int id, int organizationId, string shortName) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        ShortName = shortName,
        RegionId = RegionId,
        DistrictId = DistrictId,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };
}
