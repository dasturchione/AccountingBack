using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.PurchaseDocs;

public sealed class PurchaseDocListDtoOrderByBuilder : IOrderByBuilder<PurchaseDoc, PurchaseDocListDto>
{
    public Func<IQueryable<PurchaseDocListDto>, IOrderedQueryable<PurchaseDocListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
