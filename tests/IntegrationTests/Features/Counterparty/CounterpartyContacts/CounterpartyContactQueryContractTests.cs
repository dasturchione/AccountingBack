using Application.Abstractions.Authentication;
using Application.Features.CounterpartyContacts;
using Domain.Entities;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.CounterpartyManagement.CounterpartyContacts;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class CounterpartyContactQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 47001;
    private const int OrganizationId = 47001;
    private const int OtherOrganizationId = 47002;
    private const int RegionId = 47001;

    [Fact]
    public async Task ListAndDetailStayInsideSelectedOrganizationForSuperAdmin()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICounterpartyContactService>();

        var page = await service.GetAllAsync(new CounterpartyContactListFilter
        {
            CounterpartyId = 47101,
            Search = "scoped contact",
            Page = 2,
            PageSize = 1
        });
        var ownDetail = await service.GetByIdAsync(47201);
        var otherDetail = await service.GetByIdAsync(47203);

        Assert.True(page.IsSuccess);
        Assert.Equal(2, page.Value.TotalCount);
        Assert.Equal("Beta scoped contact", Assert.Single(page.Value.Items).FullName);
        Assert.True(ownDetail.IsSuccess);
        Assert.Equal(OrganizationId, ownDetail.Value.OrganizationId);
        Assert.Equal("Contact counterparty", ownDetail.Value.CounterpartyName);
        Assert.Equal("Active", ownDetail.Value.StateName);
        Assert.False(otherDetail.IsSuccess);
        Assert.Equal("CounterpartyContact.NotFound", otherDetail.Error.Code);
    }

    [Fact]
    public async Task CreateRequiresCounterpartyFromSelectedOrganizationAndOrganizationContext()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICounterpartyContactService>();

        var foreignCounterparty = await service.CreateAsync(new CounterpartyContactCreateDto
        {
            CounterpartyId = 47102,
            FullName = "Foreign counterparty contact"
        });

        Assert.False(foreignCounterparty.IsSuccess);
        Assert.Equal("CounterpartyContact.CounterpartyNotFound", foreignCounterparty.Error.Code);

        await using var missingProvider = CreateProvider(User(null));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missingContext = await missingScope.ServiceProvider
            .GetRequiredService<ICounterpartyContactService>()
            .CreateAsync(new CounterpartyContactCreateDto
            {
                CounterpartyId = 47101,
                FullName = "No organization contact"
            });

        Assert.False(missingContext.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missingContext.Error.Code);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<ICounterpartyContactService, CounterpartyContactService>());

    private static IntegrationTestUserContext User(int? organizationId) => new()
    {
        Id = 47001,
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
                ShortName = "Counterparty contact region",
                FullName = "Counterparty contact region",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = TenantId,
                Name = "Counterparty contact tenant",
                Slug = "counterparty-contact-contract-tenant",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Selected contact organization", "470000001"),
                Organization(OtherOrganizationId, "Other contact organization", "470000002"));
        }

        if (!await context.CounterpartyCards.IgnoreQueryFilters().AnyAsync(counterparty => counterparty.Id == 47101))
        {
            context.CounterpartyCards.AddRange(
                Counterparty(47101, OrganizationId, "Contact counterparty"),
                Counterparty(47102, OtherOrganizationId, "Foreign contact counterparty"));
        }

        if (!await context.CounterpartyContacts.IgnoreQueryFilters().AnyAsync(contact => contact.Id == 47201))
        {
            context.CounterpartyContacts.AddRange(
                Contact(47201, OrganizationId, 47101, "Alpha scoped contact"),
                Contact(47202, OrganizationId, 47101, "Beta scoped contact"),
                Contact(47203, OtherOrganizationId, 47102, "Other scoped contact"));
        }

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

    private static CounterpartyCard Counterparty(int id, int organizationId, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        ShortName = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static CounterpartyContact Contact(
        int id,
        int organizationId,
        int counterpartyId,
        string fullName) => new()
        {
            Id = id,
            OrganizationId = organizationId,
            CounterpartyId = counterpartyId,
            FullName = fullName,
            PhoneNumber = $"+99890{id}",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = SeedDate
        };
}
