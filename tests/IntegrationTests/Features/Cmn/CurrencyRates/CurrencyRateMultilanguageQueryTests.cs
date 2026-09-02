using Application.Features.Cmn.CurrencyRates;
using Domain.Entities;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Filters;
using SharedKernel.Query;

namespace IntegrationTests.Features.Cmn.CurrencyRates;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class CurrencyRateMultilanguageQueryTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const short BaseCurrencyId = 30001;
    private const short TargetCurrencyId = 30002;
    private const short FallbackCurrencyId = 30003;

    [Fact]
    public async Task ListDetailLatestAndHistoryUseRequestedCurrencyTranslations()
    {
        await SeedRatesAsync();
        var user = new IntegrationTestUserContext { LanguageId = 2 };
        await using var provider = ApplicationQueryTestServiceProvider.Create(fixture, user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICurrencyRateService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var projection = scope.ServiceProvider
            .GetRequiredService<IProjectionBuilder<CurrencyRate, CurrencyRateListDto>>()
            .Build();

        var sql = context.CurrencyRates.Select(projection).ToQueryString();
        var list = await service.GetAllAsync(new CurrencyRateListFilter
        {
            BaseCurrencyId = BaseCurrencyId,
            TargetCurrencyId = TargetCurrencyId,
            Page = 1,
            PageSize = 10
        });
        var detail = await service.GetByIdAsync(34001);
        var latest = await service.GetLatestAsync(BaseCurrencyId, TargetCurrencyId);
        var history = await service.GetHistoryAsync(
            BaseCurrencyId,
            TargetCurrencyId,
            new CurrencyRateListFilter { Page = 2, PageSize = 1 });

        Assert.True(list.IsSuccess);
        Assert.Equal(3, list.Value.TotalCount);
        Assert.All(list.Value.Items, AssertTranslatedNames);

        Assert.True(detail.IsSuccess);
        AssertTranslatedNames(detail.Value);

        Assert.True(latest.IsSuccess);
        Assert.Equal(34003, latest.Value.Id);
        AssertTranslatedNames(latest.Value);

        Assert.True(history.IsSuccess);
        Assert.Equal(3, history.Value.TotalCount);
        Assert.Equal(34002, Assert.Single(history.Value.Items).Id);
        AssertTranslatedNames(Assert.Single(history.Value.Items));

        Assert.Contains("cmn_currency_translation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke(", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TranslationFallsBackPerCurrencyAndTranslatedSearchStaysSqlCompatible()
    {
        await SeedRatesAsync();
        var user = new IntegrationTestUserContext { LanguageId = 2 };
        await using var provider = ApplicationQueryTestServiceProvider.Create(fixture, user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICurrencyRateService>();

        var fallbackDetail = await service.GetByIdAsync(34004);
        var translatedSearch = await service.GetAllAsync(new CurrencyRateListFilter
        {
            Search = "Российская базовая валюта",
            Page = 1,
            PageSize = 10
        });
        var baseNameSearch = await service.GetAllAsync(new CurrencyRateListFilter
        {
            Search = "Base currency name",
            Page = 1,
            PageSize = 10
        });

        Assert.True(fallbackDetail.IsSuccess);
        Assert.Equal("Российская базовая валюта", fallbackDetail.Value.BaseCurrencyName);
        Assert.Equal("Fallback target currency", fallbackDetail.Value.TargetCurrencyName);

        Assert.True(translatedSearch.IsSuccess);
        Assert.Equal(4, translatedSearch.Value.TotalCount);
        Assert.Equal(
            translatedSearch.Value.Items.Count,
            translatedSearch.Value.Items.Select(rate => rate.Id).Distinct().Count());

        Assert.True(baseNameSearch.IsSuccess);
        Assert.Empty(baseNameSearch.Value.Items);
    }

    [Fact]
    public async Task DynamicSortAndPagingKeepExistingSemantics()
    {
        await SeedRatesAsync();
        var user = new IntegrationTestUserContext { LanguageId = 2 };
        await using var provider = ApplicationQueryTestServiceProvider.Create(fixture, user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICurrencyRateService>();

        var result = await service.GetAllAsync(new CurrencyRateListFilter
        {
            BaseCurrencyId = BaseCurrencyId,
            TargetCurrencyId = TargetCurrencyId,
            SortBy = "officialRate",
            SortDirection = SortDirection.Asc,
            Page = 2,
            PageSize = 1
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(34002, Assert.Single(result.Value.Items).Id);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(1, result.Value.PageSize);
    }

    private static void AssertTranslatedNames(CurrencyRateDto rate)
    {
        Assert.Equal("Российская базовая валюта", rate.BaseCurrencyName);
        Assert.Equal("Российская целевая валюта", rate.TargetCurrencyName);
    }

    private async Task SeedRatesAsync()
    {
        await using var context = fixture.CreateDbContext();

        if (!await context.States.AnyAsync(state => state.Id == 1))
        {
            context.States.Add(new State
            {
                Id = 1,
                ShortName = "Active",
                FullName = "Active",
                CreatedDate = SeedDate
            });
        }

        if (!await context.Languages.AnyAsync(language => language.Id == 2))
        {
            context.Languages.Add(new Language
            {
                Id = 2,
                Code = "ru",
                Name = "Russian",
                NativeName = "Русский",
                IsDefault = false,
                SortOrder = 2,
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Currencies.AnyAsync(currency => currency.Id == BaseCurrencyId))
        {
            context.Currencies.AddRange(
                Currency(BaseCurrencyId, "RUBASE", "Base currency name", "Российская базовая валюта"),
                Currency(TargetCurrencyId, "RUTARGET", "Target currency name", "Российская целевая валюта"),
                Currency(FallbackCurrencyId, "FALLBACK", "Fallback target currency"));
        }

        if (!await context.CurrencyRates.AnyAsync(rate => rate.Id == 34001))
        {
            context.CurrencyRates.AddRange(
                Rate(34001, TargetCurrencyId, new DateTime(2026, 8, 30), 10m),
                Rate(34002, TargetCurrencyId, new DateTime(2026, 8, 31), 20m),
                Rate(34003, TargetCurrencyId, new DateTime(2026, 9, 1), 30m),
                Rate(34004, FallbackCurrencyId, new DateTime(2026, 9, 1), 40m));
        }

        await context.SaveChangesAsync();
    }

    private static Currency Currency(short id, string code, string baseName, string? russianName = null)
    {
        var currency = new Currency
        {
            Id = id,
            Code = code,
            Name = baseName,
            Symbol = code,
            StateId = 1
        };

        if (russianName is not null)
        {
            currency.CurrencyTranslations.Add(new CurrencyTranslation
            {
                CurrencyId = id,
                LanguageId = 2,
                Name = russianName
            });
        }

        return currency;
    }

    private static CurrencyRate Rate(long id, short targetCurrencyId, DateTime effectiveDate, decimal rate) => new()
    {
        Id = id,
        BaseCurrencyId = BaseCurrencyId,
        TargetCurrencyId = targetCurrencyId,
        EffectiveDate = effectiveDate,
        BuyRate = rate,
        SellRate = rate,
        OfficialRate = rate,
        RateSource = "integration-test",
        IsActive = true,
        StateId = 1,
        CreatedDate = SeedDate
    };
}
