using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.SaleDocTables;

public sealed class SaleDocTableListDtoOrderByBuilder : IOrderByBuilder<SaleDocTable, SaleDocTableListDto>
{
    public Func<IQueryable<SaleDocTableListDto>, IOrderedQueryable<SaleDocTableListDto>> Build() =>
        query => query.OrderBy(x => x.Id);
}
