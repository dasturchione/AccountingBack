using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.CurrencyRates;

public sealed class CurrencyRateListDtoProjection : IProjectionBuilder<CurrencyRate, CurrencyRateListDto>
{
    public Expression<Func<CurrencyRate, CurrencyRateListDto>> Build() =>
        x => new CurrencyRateListDto
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
            BaseCurrencyName = x.BaseCurrency.Name,
            TargetCurrencyCode = x.TargetCurrency.Code,
            TargetCurrencyName = x.TargetCurrency.Name,
            CreatedDate = x.CreatedDate
        };
}
