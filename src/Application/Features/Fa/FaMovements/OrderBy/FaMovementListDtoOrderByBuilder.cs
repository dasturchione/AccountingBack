using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.FaMovements;

public sealed class FaMovementListDtoOrderByBuilder : IOrderByBuilder<FaMovementDoc, FaMovementListDto>
{
    public Func<IQueryable<FaMovementListDto>, IOrderedQueryable<FaMovementListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
