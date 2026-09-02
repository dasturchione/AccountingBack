using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.CurrencyRates;

public sealed class CurrencyRateDtoProjection(IUserContext userContext) : IProjectionBuilder<CurrencyRate, CurrencyRateDto>
{
    public Expression<Func<CurrencyRate, CurrencyRateDto>> Build()
    {
        var languageId = userContext.LanguageId;

        return x => new CurrencyRateDto
        {
            Id = x.Id,
            BaseCurrencyId = x.BaseCurrencyId,
            TargetCurrencyId = x.TargetCurrencyId,
            EffectiveDate = x.EffectiveDate,
            BuyRate = x.BuyRate,
            SellRate = x.SellRate,
            OfficialRate = x.OfficialRate,
            RateSource = x.RateSource,
            IsActive = x.IsActive,
            StateId = x.StateId,
            StateName = x.State.FullName,
            BaseCurrencyCode = x.BaseCurrency.Code,
            BaseCurrencyName = x.BaseCurrency.CurrencyTranslations
                .Where(translation => translation.LanguageId == languageId)
                .Select(translation => translation.Name)
                .FirstOrDefault() ?? x.BaseCurrency.Name,
            TargetCurrencyCode = x.TargetCurrency.Code,
            TargetCurrencyName = x.TargetCurrency.CurrencyTranslations
                .Where(translation => translation.LanguageId == languageId)
                .Select(translation => translation.Name)
                .FirstOrDefault() ?? x.TargetCurrency.Name,
            CreatedDate = x.CreatedDate
        };
    }
}
