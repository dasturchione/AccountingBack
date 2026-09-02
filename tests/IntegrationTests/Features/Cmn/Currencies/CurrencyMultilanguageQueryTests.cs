using Application.Abstractions.Authentication;
using Application.Features.Cmn.Currencies;
using Domain.Entities;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;

namespace IntegrationTests.Features.Cmn.Currencies;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class CurrencyMultilanguageQueryTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 8, 31);

    [Fact]
    public async Task ListAndDetailUseRequestedTranslationWithBaseFallbackInSql()
    {
        await SeedCurrenciesAsync();
        var user = new IntegrationTestUserContext { LanguageId = 3 };
        await using var provider = ApplicationQueryTestServiceProvider.Create(fixture, user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICurrencyService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var projection = scope.ServiceProvider
            .GetRequiredService<IProjectionBuilder<Currency, CurrencyListDto>>()
            .Build();

        var sql = context.Currencies.Select(projection).ToQueryString();
        var listResult = await service.GetAllAsync(new CurrencyListFilter { Page = 1, PageSize = 50 });
        var detailResult = await service.GetByIdAsync(31001);

        Assert.True(listResult.IsSuccess);
        Assert.True(detailResult.IsSuccess);
        Assert.Equal("English translated sum", detailResult.Value.Name);
        Assert.Equal(
            "English translated sum",
            listResult.Value.Items.Single(currency => currency.Id == 31001).Name);
        Assert.Equal(
            "Base dollar",
            listResult.Value.Items.Single(currency => currency.Id == 31002).Name);
        Assert.Contains("cmn_currency_translation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke(", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ScopedProjectionUsesASecondRequestedLanguage()
    {
        await SeedCurrenciesAsync();
        var user = new IntegrationTestUserContext { LanguageId = 1 };
        await using var provider = ApplicationQueryTestServiceProvider.Create(fixture, user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICurrencyService>();

        var result = await service.GetByIdAsync(31001);

        Assert.True(result.IsSuccess);
        Assert.Equal("O'zbekcha so'm", result.Value.Name);
    }

    [Fact]
    public async Task TranslatedSearchOrdersAndPagesAfterProjectionWithoutDuplicates()
    {
        await SeedCurrenciesAsync();
        var user = new IntegrationTestUserContext { LanguageId = 3 };
        await using var provider = ApplicationQueryTestServiceProvider.Create(fixture, user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICurrencyService>();

        var secondPage = await service.GetAllAsync(new CurrencyListFilter
        {
            Search = "paging translated",
            Page = 2,
            PageSize = 1
        });
        var baseNameSearch = await service.GetAllAsync(new CurrencyListFilter
        {
            Search = "base paging alpha",
            Page = 1,
            PageSize = 10
        });

        Assert.True(secondPage.IsSuccess);
        Assert.Equal(3, secondPage.Value.TotalCount);
        Assert.Equal(2, secondPage.Value.Page);
        Assert.Equal(1, secondPage.Value.PageSize);
        Assert.Equal("I18NPB", Assert.Single(secondPage.Value.Items).Code);
        Assert.Equal(
            secondPage.Value.Items.Count,
            secondPage.Value.Items.Select(currency => currency.Id).Distinct().Count());

        Assert.True(baseNameSearch.IsSuccess);
        Assert.Empty(baseNameSearch.Value.Items);
    }

    private async Task SeedCurrenciesAsync()
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

        if (!await context.Languages.AnyAsync(language => language.Id == 1))
        {
            context.Languages.Add(new Language
            {
                Id = 1,
                Code = "uz",
                Name = "Uzbek",
                NativeName = "O'zbekcha",
                IsDefault = true,
                SortOrder = 1,
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Languages.AnyAsync(language => language.Id == 3))
        {
            context.Languages.Add(new Language
            {
                Id = 3,
                Code = "en",
                Name = "English",
                NativeName = "English",
                IsDefault = false,
                SortOrder = 3,
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Currencies.AnyAsync(currency => currency.Id == 31001))
        {
            context.Currencies.AddRange(
                Currency(31001, "I18NS", "Base sum", "English translated sum", "O'zbekcha so'm"),
                Currency(31002, "I18ND", "Base dollar"),
                Currency(31011, "I18NPA", "Base paging alpha", "Paging translated alpha"),
                Currency(31012, "I18NPB", "Base paging beta", "Paging translated beta"),
                Currency(31013, "I18NPC", "Base paging gamma", "Paging translated gamma"));
        }

        await context.SaveChangesAsync();
    }

    private static Currency Currency(
        short id,
        string code,
        string baseName,
        string? englishName = null,
        string? uzbekName = null)
    {
        var currency = new Currency
        {
            Id = id,
            Code = code,
            Name = baseName,
            Symbol = code,
            StateId = 1
        };

        if (englishName is not null)
        {
            currency.CurrencyTranslations.Add(new CurrencyTranslation
            {
                CurrencyId = id,
                LanguageId = 3,
                Name = englishName
            });
        }

        if (uzbekName is not null)
        {
            currency.CurrencyTranslations.Add(new CurrencyTranslation
            {
                CurrencyId = id,
                LanguageId = 1,
                Name = uzbekName
            });
        }

        return currency;
    }

}
