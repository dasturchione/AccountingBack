using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.RetailSaleDocs;

public sealed class RetailSaleDocListDtoOrderByBuilder : IOrderByBuilder<RetailSaleDoc, RetailSaleDocListDto>
{
    public Func<IQueryable<RetailSaleDocListDto>, IOrderedQueryable<RetailSaleDocListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
