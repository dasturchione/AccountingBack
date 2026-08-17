using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.FaCommissionings;

public sealed class FaCommissioningListDtoOrderByBuilder : IOrderByBuilder<FaCommissioningDoc, FaCommissioningListDto>
{
    public Func<IQueryable<FaCommissioningListDto>, IOrderedQueryable<FaCommissioningListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
