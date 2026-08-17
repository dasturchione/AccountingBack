using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.SaleDocs;

public sealed class SaleDocListDtoOrderByBuilder : IOrderByBuilder<SaleDoc, SaleDocListDto>
{
    public Func<IQueryable<SaleDocListDto>, IOrderedQueryable<SaleDocListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
