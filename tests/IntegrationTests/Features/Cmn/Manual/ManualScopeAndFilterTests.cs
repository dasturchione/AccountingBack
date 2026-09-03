using Application.Abstractions.Authentication;
using Application.Features.Manual;
using Domain.Entities;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.Cmn.Manual;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class ManualScopeAndFilterTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 39001;
    private const int OtherTenantId = 39002;
    private const int OrganizationId = 39001;
    private const int SecondOrganizationId = 39002;
    private const int OtherOrganizationId = 39003;
    private const int UserId = 39001;

    [Fact]
    public async Task OrganizationsRespectSuperAdminTenantAdminAndTenantUserScope()
    {
        await SeedAsync();

        var superAdmin = await GetOrganizationsAsync(User(CurrentUserKind.SuperAdmin, null, null, []));
        var tenantAdmin = await GetOrganizationsAsync(User(CurrentUserKind.TenantAdmin, TenantId, null, []));
        var tenantUser = await GetOrganizationsAsync(User(
            CurrentUserKind.TenantUser,
            TenantId,
            OrganizationId,
            [OrganizationId, SecondOrganizationId]));

        Assert.Equal(
            [SecondOrganizationId, OtherOrganizationId, OrganizationId],
            OwnedOrganizationIds(superAdmin));
        Assert.Equal(
            [SecondOrganizationId, OrganizationId],
            OwnedOrganizationIds(tenantAdmin));
        Assert.Equal([SecondOrganizationId], OwnedOrganizationIds(tenantUser));
    }

    [Fact]
    public async Task OrganizationResourcesAndOptionalFiltersDoNotLeakOtherOrganizations()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(
            CurrentUserKind.TenantUser,
            TenantId,
            OrganizationId,
            [OrganizationId]));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IManualService>();

        AssertOwnedOnly(await service.GetBranchesAsync(), 39101, 39102);
        AssertOwnedOnly(await service.GetFaGroupsAsync(), 39111, 39112);
        AssertOwnedOnly(await service.GetWarehousesAsync(39101), 39121, 39122);
        AssertOwnedOnly(await service.GetCashBoxesAsync(39101), 39131, 39132);
        AssertOwnedOnly(await service.GetChartAccountsAsync(), 39141, 39142);
        AssertOwnedOnly(await service.GetOrgBankAccountsAsync(), 39151, 39152);
        AssertOwnedOnly(await service.GetPaymentAcceptancePointsAsync(), 39161, 39162);

        var branches = await service.GetBankBranchesAsync(39201);
        Assert.Contains(branches, branch => branch.Id == 39211 && branch.BankId == 39201 && branch.Mfo == "00921");
        Assert.DoesNotContain(branches, branch => branch.Id == 39212);

        var counterpartyAccounts = await service.GetCounterpartyBankAccountsAsync(39221, 39201);
        Assert.Contains(counterpartyAccounts, account => account.Id == 39231);
        Assert.DoesNotContain(counterpartyAccounts, account => account.Id is 39232 or 39233);

        var products = await service.GetProductsAsync(
            productGroupId: 39241,
            warehouseId: 39121,
            isService: false,
            isSold: true,
            isPurchased: true);
        var product = Assert.Single(products, item => item.Id is 39251 or 39252 or 39253);
        Assert.Equal(39251, product.Id);
        Assert.Equal("PCS-MANUAL", product.UnitCode);
    }

    [Fact]
    public async Task ModuleGroupsAndModulesHaveDeterministicOrderAndExcludePassiveModules()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(
            CurrentUserKind.SuperAdmin,
            null,
            null,
            []));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IManualService>();

        var result = await service.GetModuleSubGroupSelectListAsync();
        var owned = result.Where(group => group.Id is 39301 or 39302).ToList();

        Assert.Collection(
            owned,
            group =>
            {
                Assert.Equal(39302, group.Id);
                Assert.Equal("AA Manual group", group.FullName);
                Assert.Equal([39402], group.Modules.Select(module => module.Id));
            },
            group =>
            {
                Assert.Equal(39301, group.Id);
                Assert.Equal("ZZ Manual group", group.FullName);
                Assert.Equal([39401, 39403], group.Modules.Select(module => module.Id));
            });
        Assert.DoesNotContain(owned.SelectMany(group => group.Modules), module => module.Id == 39404);
    }

    [Fact]
    public async Task RegionsAndDistrictsExposeTheirCodes()
    {
        await using (var context = fixture.CreateDbContext())
        {
            await SeedCoreAsync(context);
            await context.SaveChangesAsync();

            var seededRegion = await context.Regions.SingleAsync(entity => entity.Id == 39001);
            seededRegion.Code = "26";

            var seededDistrict = await context.Districts.SingleOrDefaultAsync(entity => entity.Id == 39001);
            if (seededDistrict is null)
            {
                context.Districts.Add(new District
                {
                    Id = 39001,
                    Code = "7",
                    ShortName = "Manual district",
                    FullName = "Manual district",
                    RegionId = 39001,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = SeedDate
                });
            }
            else
            {
                seededDistrict.Code = "7";
            }

            await context.SaveChangesAsync();
        }

        await using var provider = CreateProvider(User(CurrentUserKind.SuperAdmin, null, null, []));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IManualService>();

        var region = Assert.Single(await service.GetRegionsAsync(), value => value.Id == 39001);
        var district = Assert.Single(await service.GetDistrictsAsync(39001), value => value.Id == 39001);

        Assert.Equal("26", region.Code);
        Assert.Equal("7", district.Code);
    }

    private async Task<List<SelectListDto>> GetOrganizationsAsync(IntegrationTestUserContext user)
    {
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IManualService>().GetOrganizationsAsync();
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<IManualService, ManualService>());

    private static IntegrationTestUserContext User(
        CurrentUserKind userKind,
        int? tenantId,
        int? organizationId,
        List<int> allowedOrganizations) => new()
    {
        Id = UserId,
        UserKind = userKind,
        LanguageId = LanguageIdConst.RU,
        TenantId = tenantId,
        OrganizationId = organizationId,
        AllowedOrganizationIds = allowedOrganizations
    };

    private static int[] OwnedOrganizationIds(IEnumerable<SelectListDto> values) => values
        .Where(value => value.Id is OrganizationId or SecondOrganizationId or OtherOrganizationId)
        .Select(value => (int)value.Id)
        .ToArray();

    private static void AssertOwnedOnly<T>(IEnumerable<T> values, long expectedId, long forbiddenId)
        where T : SelectListDto
    {
        Assert.Contains(values, value => value.Id == expectedId);
        Assert.DoesNotContain(values, value => value.Id == forbiddenId);
    }

    private async Task SeedAsync()
    {
        await using var context = fixture.CreateDbContext();

        await SeedCoreAsync(context);
        await SeedOrganizationsAsync(context);
        await SeedScopedResourcesAsync(context);
        await SeedFilteredResourcesAsync(context);
        await SeedModulesAsync(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCoreAsync(AppDbContext context)
    {
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

        if (!await context.States.AnyAsync(state => state.Id == StateIdConst.PASSIVE))
        {
            context.States.Add(new State
            {
                Id = StateIdConst.PASSIVE,
                ShortName = "Passive",
                FullName = "Passive",
                CreatedDate = SeedDate
            });
        }

        if (!await context.Regions.AnyAsync(region => region.Id == 39001))
        {
            context.Regions.Add(new Region
            {
                Id = 39001,
                ShortName = "Manual region",
                FullName = "Manual region",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.AddRange(
                new PlatformTenant
                {
                    Id = TenantId,
                    Name = "Manual tenant",
                    Slug = "manual-scope-tenant",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = SeedDate
                },
                new PlatformTenant
                {
                    Id = OtherTenantId,
                    Name = "Other manual tenant",
                    Slug = "other-manual-scope-tenant",
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = SeedDate
                });
        }

        if (!await context.Currencies.AnyAsync(currency => currency.Id == 25201))
        {
            context.Currencies.Add(new Currency
            {
                Id = 25201,
                Code = "MSC",
                Name = "Manual scope currency",
                StateId = StateIdConst.ACTIVE
            });
        }

        if (!await context.Units.AnyAsync(unit => unit.Id == 25201))
        {
            context.Units.Add(new Unit
            {
                Id = 25201,
                Code = "PCS-MANUAL",
                Name = "Manual pieces",
                StateId = StateIdConst.ACTIVE
            });
        }

        if (!await context.DocumentTypes.AnyAsync(type => type.Id == 25201))
        {
            context.DocumentTypes.Add(new DocumentType
            {
                Id = 25201,
                Code = "manual_scope_doc",
                Name = "Manual scope document",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.MovementDirections.AnyAsync(direction => direction.Id == 25201))
        {
            context.MovementDirections.Add(new MovementDirection
            {
                Id = 25201,
                Code = "MSIN",
                Name = "Manual scope inbound"
            });
        }

        if (!await context.Set<UserKind>().AnyAsync(kind => kind.Id == 25201))
        {
            context.Set<UserKind>().Add(new UserKind
            {
                Id = 25201,
                Code = "MANUAL_USER",
                Name = "Manual user"
            });
        }

        if (!await context.PaymentAcceptancePointTypes.AnyAsync(type => type.Id == 25201))
        {
            context.PaymentAcceptancePointTypes.Add(new PaymentAcceptancePointType
            {
                Id = 25201,
                Code = "MANUAL_POINT",
                Name = "Manual point type",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }
    }

    private static async Task SeedOrganizationsAsync(AppDbContext context)
    {
        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(org => org.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, TenantId, "ZZ Manual organization", "390000001"),
                Organization(SecondOrganizationId, TenantId, "AA Manual organization", "390000002"),
                Organization(OtherOrganizationId, OtherTenantId, "MM Other organization", "390000003"));
        }

        if (!await context.Set<User>().IgnoreQueryFilters().AnyAsync(user => user.Id == UserId))
        {
            context.Set<User>().Add(new User
            {
                Id = UserId,
                UserName = "manual-scope-user",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                PhoneNumber = "+998900000001",
                FirstName = "Manual",
                LastName = "User",
                TenantId = TenantId,
                UserKindId = 25201,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
            context.UserOrganizations.Add(new UserOrganization
            {
                UserId = UserId,
                OrganizationId = SecondOrganizationId,
                IsDefault = true,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate,
                IsOwner = false,
                JoinedAt = SeedDate
            });
        }
    }

    private static async Task SeedScopedResourcesAsync(AppDbContext context)
    {
        if (!await context.Branches.IgnoreQueryFilters().AnyAsync(branch => branch.Id == 39101))
        {
            context.Branches.AddRange(
                Branch(39101, OrganizationId, "BR-OWN", "Owned branch"),
                Branch(39102, OtherOrganizationId, "BR-OTHER", "Other branch"));
        }

        if (!await context.FaGroups.IgnoreQueryFilters().AnyAsync(group => group.Id == 39111))
        {
            context.FaGroups.AddRange(
                FaGroup(39111, OrganizationId, "FA-OWN", "Owned FA group"),
                FaGroup(39112, OtherOrganizationId, "FA-OTHER", "Other FA group"));
        }

        if (!await context.Warehouses.IgnoreQueryFilters().AnyAsync(warehouse => warehouse.Id == 39121))
        {
            context.Warehouses.AddRange(
                Warehouse(39121, OrganizationId, 39101, "Owned warehouse"),
                Warehouse(39122, OtherOrganizationId, 39102, "Other warehouse"));
        }

        if (!await context.CashBoxes.IgnoreQueryFilters().AnyAsync(cash => cash.Id == 39131))
        {
            context.CashBoxes.AddRange(
                CashBox(39131, OrganizationId, 39101, "CASH-OWN", "Owned cash"),
                CashBox(39132, OtherOrganizationId, 39102, "CASH-OTHER", "Other cash"));
        }

        if (!await context.ChartAccounts.IgnoreQueryFilters().AnyAsync(account => account.Id == 39141))
        {
            context.ChartAccounts.AddRange(
                ChartAccount(39141, OrganizationId, "99101", "Owned chart account"),
                ChartAccount(39142, OtherOrganizationId, "99102", "Other chart account"));
        }

        if (!await context.Banks.AnyAsync(bank => bank.Id == 39201))
        {
            context.Banks.AddRange(
                Bank(39201, "MANUALBANK1", "Manual bank one"),
                Bank(39202, "MANUALBANK2", "Manual bank two"));
        }

        if (!await context.BankAccounts.IgnoreQueryFilters().AnyAsync(account => account.Id == 39151))
        {
            context.BankAccounts.AddRange(
                BankAccount(39151, OrganizationId, 39201, "202080001"),
                BankAccount(39152, OtherOrganizationId, 39201, "202080002"));
        }

        if (!await context.PaymentAcceptancePoints.IgnoreQueryFilters().AnyAsync(point => point.Id == 39161))
        {
            context.PaymentAcceptancePoints.AddRange(
                AcceptancePoint(39161, OrganizationId, 39151, "POINT-OWN", "Owned point"),
                AcceptancePoint(39162, OtherOrganizationId, 39152, "POINT-OTHER", "Other point"));
        }
    }

    private static async Task SeedFilteredResourcesAsync(AppDbContext context)
    {
        if (!await context.BankBranches.AnyAsync(branch => branch.Id == 39211))
        {
            context.BankBranches.AddRange(
                BankBranch(39211, 39201, "00921", "First bank branch"),
                BankBranch(39212, 39202, "00922", "Second bank branch"));
        }

        if (!await context.CounterpartyCards.IgnoreQueryFilters().AnyAsync(card => card.Id == 39221))
        {
            context.CounterpartyCards.AddRange(
                Counterparty(39221, OrganizationId, "Owned counterparty"),
                Counterparty(39222, OtherOrganizationId, "Other counterparty"));
        }

        if (!await context.CounterpartyBankAccounts.IgnoreQueryFilters().AnyAsync(account => account.Id == 39231))
        {
            context.CounterpartyBankAccounts.AddRange(
                CounterpartyAccount(39231, OrganizationId, 39221, 39201, "202090001"),
                CounterpartyAccount(39232, OrganizationId, 39221, 39202, "202090002"),
                CounterpartyAccount(39233, OtherOrganizationId, 39222, 39201, "202090003"));
        }

        if (!await context.ProductGroups.AnyAsync(group => group.Id == 39241))
        {
            context.ProductGroups.AddRange(
                ProductGroup(39241, "PG-ONE", "Product group one"),
                ProductGroup(39242, "PG-TWO", "Product group two"));
        }

        if (!await context.Products.IgnoreQueryFilters().AnyAsync(product => product.Id == 39251))
        {
            context.Products.AddRange(
                Product(39251, OrganizationId, 39241, "Owned selected product", false, true, true),
                Product(39252, OrganizationId, 39242, "Owned filtered product", true, true, false),
                Product(39253, OtherOrganizationId, 39241, "Other organization product", false, true, true));
            context.WarehouseProductMovements.Add(new WarehouseProductMovement
            {
                Id = 39261,
                OrganizationId = OrganizationId,
                WarehouseId = 39121,
                ProductId = 39251,
                DocumentTypeId = 25201,
                DocumentId = 1,
                Quantity = 1m,
                DirectionId = 25201,
                MovementDate = SeedDate,
                CreatedDate = SeedDate
            });
        }
    }

    private static async Task SeedModulesAsync(AppDbContext context)
    {
        if (!await context.ModuleSubGroups.AnyAsync(group => group.Id == 39301))
        {
            context.ModuleSubGroups.AddRange(
                new ModuleSubGroup { Id = 39301, Code = "MG-Z", ShortName = "Z", FullName = "ZZ Manual group", CreatedDate = SeedDate },
                new ModuleSubGroup { Id = 39302, Code = "MG-A", ShortName = "A", FullName = "AA Manual group", CreatedDate = SeedDate });
        }

        if (!await context.Modules.AnyAsync(module => module.Id == 39401))
        {
            context.Modules.AddRange(
                Module(39403, 39301, "MOD-Z-2", "Z second", StateIdConst.ACTIVE),
                Module(39401, 39301, "MOD-Z-1", "Z first", StateIdConst.ACTIVE),
                Module(39402, 39302, "MOD-A-1", "A first", StateIdConst.ACTIVE),
                Module(39404, 39302, "MOD-A-X", "A passive", StateIdConst.PASSIVE));
        }
    }

    private static Organization Organization(int id, int tenantId, string name, string inn) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = inn,
        RegionId = 39001,
        IsParent = false,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate,
        TenantId = tenantId,
        SetupStatus = "completed"
    };

    private static Branch Branch(int id, int organizationId, string code, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        Code = code,
        Name = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static FaGroup FaGroup(int id, int organizationId, string code, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        Code = code,
        Name = name,
        StateId = StateIdConst.ACTIVE
    };

    private static Warehouse Warehouse(int id, int organizationId, int branchId, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        BranchId = branchId,
        Name = name,
        Code = $"WH-{id}",
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static CashBox CashBox(int id, int organizationId, int branchId, string code, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        BranchId = branchId,
        Code = code,
        Name = name,
        CurrencyId = 25201,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static ChartAccount ChartAccount(int id, int organizationId, string number, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        Code = $"CA-{id}",
        Number = number,
        Name = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static Bank Bank(int id, string code, string name) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static BankAccount BankAccount(int id, int organizationId, int bankId, string number) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        BankId = bankId,
        AccountNumber = number,
        CurrencyId = 25201,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static PaymentAcceptancePoint AcceptancePoint(int id, int organizationId, int bankAccountId, string code, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        TypeId = 25201,
        BankAccountId = bankAccountId,
        Code = code,
        Name = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static BankBranch BankBranch(int id, int bankId, string mfo, string name) => new()
    {
        Id = id,
        BankId = bankId,
        Mfo = mfo,
        BranchType = 1,
        Name = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static CounterpartyCard Counterparty(int id, int organizationId, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        ShortName = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static CounterpartyBankAccount CounterpartyAccount(
        int id,
        int organizationId,
        int counterpartyId,
        int bankId,
        string number) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        CounterpartyId = counterpartyId,
        BankId = bankId,
        AccountNumber = number,
        CurrencyId = 25201,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static ProductGroup ProductGroup(int id, string code, string name) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        StateId = StateIdConst.ACTIVE,
        IsAssignable = true,
        SortOrder = id,
        CreatedDate = SeedDate
    };

    private static Product Product(
        int id,
        int organizationId,
        int groupId,
        string name,
        bool isService,
        bool isSold,
        bool isPurchased) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        ProductGroupId = groupId,
        UnitId = 25201,
        Name = name,
        IsService = isService,
        IsSold = isSold,
        IsPurchased = isPurchased,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static Module Module(int id, int subGroupId, string code, string name, short stateId) => new()
    {
        Id = id,
        SubGroupId = subGroupId,
        Code = code,
        ShortName = name,
        FullName = name,
        StateId = stateId,
        SortOrder = id,
        IsVisible = true,
        CreatedDate = SeedDate
    };
}
