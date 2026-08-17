using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Cmn.CurrencyRates;

public sealed class CurrencyRateListDtoOrderByBuilder : IOrderByBuilder<CurrencyRate, CurrencyRateListDto>
{
    public Func<IQueryable<CurrencyRateListDto>, IOrderedQueryable<CurrencyRateListDto>> Build() =>
        query => query.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id);
}
