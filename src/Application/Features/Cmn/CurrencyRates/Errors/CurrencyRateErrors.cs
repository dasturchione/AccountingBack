using SharedKernel.Results;

namespace Application.Features.Cmn.CurrencyRates;

public static class CurrencyRateErrors
{
    public static Error NotFound(long id, short languageId) =>
        Error.NotFound("CurrencyRate.NotFound", $"Currency rate with id '{id}' not found.");

    public static Error LatestNotFound(short baseCurrencyId, short targetCurrencyId, short languageId) =>
        Error.NotFound("CurrencyRate.LatestNotFound", $"Latest rate for currency pair '{baseCurrencyId}' -> '{targetCurrencyId}' not found.");

    public static Error SameCurrency(short languageId) =>
        Error.Business("CurrencyRate.SameCurrency", "Base and target currencies must be different.");

    public static Error InvalidRates(short languageId) =>
        Error.Business("CurrencyRate.InvalidRates", "All exchange rates must be greater than zero.");

    public static Error FutureEffectiveDate(short languageId) =>
        Error.Business("CurrencyRate.FutureEffectiveDate", "Effective date cannot be in the future.");

    public static Error DuplicateEffectiveDate(short languageId) =>
        Error.Conflict("CurrencyRate.DuplicateEffectiveDate", "A rate already exists for the same currency pair and effective date.");

    public static Error InactiveCurrency(short currencyId, short languageId) =>
        Error.Business("CurrencyRate.InactiveCurrency", $"Currency '{currencyId}' is inactive.");

    public static Error MissingCurrency(short currencyId, short languageId) =>
        Error.NotFound("CurrencyRate.MissingCurrency", $"Currency '{currencyId}' was not found.");

    public static Error InvalidProviderRow(short languageId) =>
        Error.Business("CurrencyRate.InvalidProviderRow", "Provider row is invalid.");
}
