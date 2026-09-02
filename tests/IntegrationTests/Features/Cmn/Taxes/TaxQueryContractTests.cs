using Application.Abstractions.Authentication;
using Application.Features.Cmn.Taxes;
using Domain.Entities;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace IntegrationTests.Features.Cmn.Taxes;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class TaxQueryContractTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int OrganizationId = 38001;
    private const int OtherOrganizationId = 38002;

    [Fact]
    public async Task ListUsesBaseNamesAndAppliesStateSearchOrderAndPagingInSql()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User());
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ITaxService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var projection = scope.ServiceProvider
            .GetRequiredService<IProjectionBuilder<VatRate, TaxListDto>>()
            .Build();

        var sql = context.VatRates.Select(projection).ToQueryString();
        var result = await service.GetPagedAsync(new TaxListFilter
        {
            StateId = StateIdConst.ACTIVE,
            Search = "tax query",
            Page = 1,
            PageSize = 1
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        var tax = Assert.Single(result.Value.Items);
        Assert.Equal((short)26002, tax.Id);
        Assert.Equal("Tax Query Alpha", tax.Name);
        Assert.Equal(12m, tax.Rate);
        Assert.DoesNotContain("_translation", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResolverAndCalculatorUseOrganizationScopeEffectiveDateAndExistingFormulas()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User());
        await using var scope = provider.CreateAsyncScope();
        var resolver = scope.ServiceProvider.GetRequiredService<ITaxResolverService>();
        var calculator = scope.ServiceProvider.GetRequiredService<ITaxCalculationService>();

        var resolved = await resolver.ResolveAsync(
            OrganizationId,
            TaxTypeIdConst.VAT,
            new DateOnly(2026, 9, 2));
        var forbidden = await resolver.ResolveAsync(
            OtherOrganizationId,
            TaxTypeIdConst.VAT,
            new DateOnly(2026, 9, 2));
        var exclusive = await calculator.CalculateAsync(new TaxCalculationRequestDto
        {
            OrganizationId = OrganizationId,
            TaxTypeId = TaxTypeIdConst.VAT,
            Amount = 100m,
            CalculationMode = TaxCalculationMode.Exclusive,
            EffectiveDate = new DateOnly(2026, 9, 2)
        });
        var inclusive = await calculator.CalculateAsync(new TaxCalculationRequestDto
        {
            OrganizationId = OrganizationId,
            TaxTypeId = TaxTypeIdConst.VAT,
            Amount = 112m,
            CalculationMode = TaxCalculationMode.Inclusive,
            EffectiveDate = new DateOnly(2026, 9, 2)
        });

        Assert.True(resolved.IsSuccess);
        Assert.Equal((short)26002, resolved.Value.VatRateId);
        Assert.Equal(12m, resolved.Value.Rate);
        Assert.False(forbidden.IsSuccess);
        Assert.Equal("Common.Forbidden", forbidden.Error.Code);

        Assert.True(exclusive.IsSuccess);
        Assert.Equal(100m, exclusive.Value.BaseAmount);
        Assert.Equal(12m, exclusive.Value.TaxAmount);
        Assert.Equal(112m, exclusive.Value.TotalAmount);

        Assert.True(inclusive.IsSuccess);
        Assert.Equal(100m, inclusive.Value.BaseAmount);
        Assert.Equal(12m, inclusive.Value.TaxAmount);
        Assert.Equal(112m, inclusive.Value.TotalAmount);
    }

    [Fact]
    public async Task MissingDetailUsesRequestedLanguage()
    {
        await SeedAsync();
        await using var provider = CreateProvider(User());
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ITaxService>();

        var result = await service.GetByIdAsync(26999);

        Assert.False(result.IsSuccess);
        Assert.Equal("Tax.NotFound", result.Error.Code);
        Assert.Equal("Налоговая ставка с id 26999 не найдена.", result.Error.Description);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services =>
            {
                services.AddScoped<ITaxService, TaxService>();
                services.AddScoped<ITaxResolverService, TaxResolverService>();
                services.AddScoped<ITaxCalculationService, TaxCalculationService>();
            });

    private static IntegrationTestUserContext User() => new()
    {
        Id = 38001,
        UserKind = CurrentUserKind.TenantUser,
        LanguageId = LanguageIdConst.RU,
        TenantId = 38001,
        OrganizationId = OrganizationId,
        AllowedOrganizationIds = [OrganizationId]
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

        if (!await context.TaxTypes.AnyAsync(taxType => taxType.Id == TaxTypeIdConst.VAT))
        {
            context.TaxTypes.Add(new TaxType
            {
                Id = TaxTypeIdConst.VAT,
                Code = "VAT",
                Name = "VAT",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }

        if (!await context.VatRates.AnyAsync(rate => rate.Id == 26001))
        {
            context.VatRates.AddRange(
                Rate(26001, "TAX-CONTRACT-ZETA", "Tax Query Zeta", 10m,
                    new DateOnly(2024, 1, 1), new DateOnly(2026, 8, 31), StateIdConst.ACTIVE),
                Rate(26002, "TAX-CONTRACT-ALPHA", "Tax Query Alpha", 12m,
                    new DateOnly(2026, 9, 1), null, StateIdConst.ACTIVE),
                Rate(26003, "TAX-CONTRACT-PASSIVE", "Tax Query Passive", 15m,
                    new DateOnly(2026, 9, 1), null, StateIdConst.PASSIVE));
        }

        if (!await context.OrganizationTaxSettings.IgnoreQueryFilters()
                .AnyAsync(setting => setting.Id == 38001))
        {
            context.OrganizationTaxSettings.AddRange(
                Setting(38001, OrganizationId),
                Setting(38002, OtherOrganizationId));
        }

        await context.SaveChangesAsync();
    }

    private static VatRate Rate(
        short id,
        string code,
        string name,
        decimal rate,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        short stateId) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        Rate = rate,
        EffectiveFrom = effectiveFrom,
        EffectiveTo = effectiveTo,
        StateId = stateId,
        CreatedDate = SeedDate
    };

    private static OrganizationTaxSetting Setting(int id, int organizationId) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        TaxTypeId = TaxTypeIdConst.VAT,
        IsVatPayer = true,
        EffectiveFrom = new DateOnly(2026, 1, 1),
        StateId = StateIdConst.ACTIVE,
        CreatedDate = SeedDate
    };
}
