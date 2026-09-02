using Application.Abstractions.Authentication;
using Application.Features.CounterpartyBankAccounts;
using Domain.Entities;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.CounterpartyManagement.CounterpartyBankAccounts;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class CounterpartyBankAccountQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int TenantId = 46001;
    private const int OrganizationId = 46001;
    private const int OtherOrganizationId = 46002;
    private const int RegionId = 46001;
    private const int BankId = 46001;
    private const int BankBranchId = 46001;
    private const short CurrencyId = 31021;
    private const short FallbackCurrencyId = 31022;

    [Fact]
    public async Task ListAndDetailAreScopedAndUseRequestedCurrencyTranslation()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICounterpartyBankAccountService>();

        var page = await service.GetAllAsync(new CounterpartyBankAccountListFilter
        {
            CounterpartyId = 46101,
            Search = "SCOPED-ACCOUNT",
            Page = 2,
            PageSize = 1
        });
        var ownDetail = await service.GetByIdAsync(46201);
        var fallbackDetail = await service.GetByIdAsync(46205);
        var otherDetail = await service.GetByIdAsync(46203);

        Assert.True(page.IsSuccess);
        Assert.Equal(2, page.Value.TotalCount);
        Assert.Equal("SCOPED-ACCOUNT-B", Assert.Single(page.Value.Items).AccountNumber);
        Assert.True(ownDetail.IsSuccess);
        Assert.Equal(OrganizationId, ownDetail.Value.OrganizationId);
        Assert.Equal("Тестовая валюта счёта", ownDetail.Value.CurrencyName);
        Assert.Equal("Тестовая валюта счёта", Assert.Single(page.Value.Items).CurrencyName);
        Assert.Equal("Counterparty bank", ownDetail.Value.BankName);
        Assert.Equal("Active", ownDetail.Value.StateName);
        Assert.True(fallbackDetail.IsSuccess);
        Assert.Equal("Fallback account currency", fallbackDetail.Value.CurrencyName);
        Assert.False(otherDetail.IsSuccess);
        Assert.Equal("CounterpartyBankAccount.NotFound", otherDetail.Error.Code);
    }

    [Fact]
    public async Task AccountNumberIsOrganizationLocalAndCounterpartyMustBelongToOrganization()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User(OrganizationId));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICounterpartyBankAccountService>();

        var created = await service.CreateAsync(CreateDto(46101, "CROSS-ORG-ACCOUNT"));
        var foreignCounterparty = await service.CreateAsync(CreateDto(46102, "FOREIGN-COUNTERPARTY-ACCOUNT"));

        Assert.True(created.IsSuccess);
        Assert.False(foreignCounterparty.IsSuccess);
        Assert.Equal("CounterpartyBankAccount.CounterpartyNotFound", foreignCounterparty.Error.Code);

        await using var missingProvider = CreateProvider(User(null));
        await using var missingScope = missingProvider.CreateAsyncScope();
        var missingContext = await missingScope.ServiceProvider
            .GetRequiredService<ICounterpartyBankAccountService>()
            .CreateAsync(CreateDto(46101, "NO-ORG-ACCOUNT"));

        Assert.False(missingContext.IsSuccess);
        Assert.Equal("Common.UserHasNoOrganization", missingContext.Error.Code);
    }

    private static CounterpartyBankAccountCreateDto CreateDto(int counterpartyId, string accountNumber) => new()
    {
        CounterpartyId = counterpartyId,
        BankId = BankId,
        BankBranchId = BankBranchId,
        AccountNumber = accountNumber,
        CurrencyId = CurrencyId
    };

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<ICounterpartyBankAccountService, CounterpartyBankAccountService>());

    private static IntegrationTestUserContext User(int? organizationId) => new()
    {
        Id = 46001,
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
            context.Regions.Add(new Region
            {
                Id = RegionId,
                ShortName = "Counterparty account region",
                FullName = "Counterparty account region",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == TenantId))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = TenantId,
                Name = "Counterparty account tenant",
                Slug = "counterparty-account-contract-tenant",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Selected account organization", "460000001"),
                Organization(OtherOrganizationId, "Other account organization", "460000002"));
        }

        if (!await context.Banks.AnyAsync(bank => bank.Id == BankId))
        {
            context.Banks.Add(new Bank
            {
                Id = BankId,
                Code = "COUNTERPARTY_ACCOUNT_BANK",
                Name = "Counterparty bank",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.BankBranches.AnyAsync(branch => branch.Id == BankBranchId))
        {
            context.BankBranches.Add(new BankBranch
            {
                Id = BankBranchId,
                BankId = BankId,
                Mfo = "46001",
                BranchType = 1,
                Name = "Counterparty bank branch",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Currencies.AnyAsync(currency => currency.Id == CurrencyId))
        {
            var currency = new Currency
            {
                Id = CurrencyId,
                Code = "CPACC",
                Name = "Base account currency",
                Symbol = "CA",
                StateId = StateIdConst.ACTIVE
            };
            currency.CurrencyTranslations.Add(new CurrencyTranslation
            {
                CurrencyId = CurrencyId,
                LanguageId = LanguageIdConst.RU,
                Name = "Тестовая валюта счёта"
            });
            context.Currencies.Add(currency);
        }

        if (!await context.Currencies.AnyAsync(currency => currency.Id == FallbackCurrencyId))
        {
            context.Currencies.Add(new Currency
            {
                Id = FallbackCurrencyId,
                Code = "CPACF",
                Name = "Fallback account currency",
                Symbol = "CF",
                StateId = StateIdConst.ACTIVE
            });
        }

        if (!await context.CounterpartyCards.IgnoreQueryFilters().AnyAsync(counterparty => counterparty.Id == 46101))
        {
            context.CounterpartyCards.AddRange(
                Counterparty(46101, OrganizationId, "Account counterparty"),
                Counterparty(46102, OtherOrganizationId, "Foreign account counterparty"));
        }

        if (!await context.CounterpartyBankAccounts.IgnoreQueryFilters().AnyAsync(account => account.Id == 46201))
        {
            context.CounterpartyBankAccounts.AddRange(
                Account(46201, OrganizationId, 46101, "SCOPED-ACCOUNT-A", true),
                Account(46202, OrganizationId, 46101, "SCOPED-ACCOUNT-B", false),
                Account(46203, OtherOrganizationId, 46102, "SCOPED-ACCOUNT-OTHER", true),
                Account(46204, OtherOrganizationId, 46102, "CROSS-ORG-ACCOUNT", false));
        }

        if (!await context.CounterpartyBankAccounts.IgnoreQueryFilters().AnyAsync(account => account.Id == 46205))
        {
            context.CounterpartyBankAccounts.Add(
                Account(46205, OrganizationId, 46101, "BASE-CURRENCY-ACCOUNT", false, FallbackCurrencyId));
        }

        await context.CounterpartyBankAccounts.IgnoreQueryFilters()
            .Where(account => account.OrganizationId == OrganizationId &&
                              (account.AccountNumber == "CROSS-ORG-ACCOUNT" ||
                               account.AccountNumber == "FOREIGN-COUNTERPARTY-ACCOUNT"))
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

    private static CounterpartyCard Counterparty(int id, int organizationId, string name) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        ShortName = name,
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };

    private static CounterpartyBankAccount Account(
        int id,
        int organizationId,
        int counterpartyId,
        string accountNumber,
        bool isMain,
        short currencyId = CurrencyId) => new()
        {
            Id = id,
            OrganizationId = organizationId,
            CounterpartyId = counterpartyId,
            BankId = BankId,
            BankBranchId = BankBranchId,
            AccountNumber = accountNumber,
            CurrencyId = currencyId,
            IsMain = isMain,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = SeedDate
        };
}
