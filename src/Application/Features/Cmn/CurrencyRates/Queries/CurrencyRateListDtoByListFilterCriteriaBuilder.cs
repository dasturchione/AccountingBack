using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.CurrencyRates;

public sealed class CurrencyRateListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<CurrencyRateListDto, CurrencyRateListFilter>
{
    public Expression<Func<CurrencyRateListDto, bool>> Build(CurrencyRateListFilter options) =>
        x => (options.BaseCurrencyId == null || x.BaseCurrencyId == options.BaseCurrencyId) &&
             (options.TargetCurrencyId == null || x.TargetCurrencyId == options.TargetCurrencyId) &&
             (options.IsActive == null || x.IsActive == options.IsActive) &&
             (options.EffectiveFrom == null || x.EffectiveDate >= options.EffectiveFrom) &&
             (options.EffectiveTo == null || x.EffectiveDate <= options.EffectiveTo) &&
             (string.IsNullOrWhiteSpace(options.Search) ||
              x.BaseCurrencyCode.ToLower().Contains(options.Search.ToLower()) ||
              x.BaseCurrencyName.ToLower().Contains(options.Search.ToLower()) ||
              x.TargetCurrencyCode.ToLower().Contains(options.Search.ToLower()) ||
              x.TargetCurrencyName.ToLower().Contains(options.Search.ToLower()) ||
              (x.RateSource != null && x.RateSource.ToLower().Contains(options.Search.ToLower())));
}
