using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Cmn.Currencies;

public sealed class CurrencyListDtoOrderByBuilder : IOrderByBuilder<Currency, CurrencyListDto>
{
    public Func<IQueryable<CurrencyListDto>, IOrderedQueryable<CurrencyListDto>> Build() =>
        query => query.OrderBy(x => x.Code).ThenBy(x => x.Id);
}
