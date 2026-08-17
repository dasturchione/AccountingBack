using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxListDtoOrderByBuilder : IOrderByBuilder<VatRate, TaxListDto>
{
    public Func<IQueryable<TaxListDto>, IOrderedQueryable<TaxListDto>> Build() =>
        query => query.OrderBy(x => x.Name).ThenBy(x => x.Id);
}
