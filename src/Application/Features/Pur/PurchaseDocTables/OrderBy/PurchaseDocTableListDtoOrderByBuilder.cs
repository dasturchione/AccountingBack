using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.PurchaseDocTables;

public sealed class PurchaseDocTableListDtoOrderByBuilder : IOrderByBuilder<PurchaseDocTable, PurchaseDocTableListDto>
{
    public Func<IQueryable<PurchaseDocTableListDto>, IOrderedQueryable<PurchaseDocTableListDto>> Build() =>
        query => query.OrderBy(x => x.Id);
}
