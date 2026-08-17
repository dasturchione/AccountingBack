using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Positions;

public sealed class PositionListDtoOrderByBuilder : IOrderByBuilder<Position, PositionListDto>
{
    public Func<IQueryable<PositionListDto>, IOrderedQueryable<PositionListDto>> Build() =>
        query => query.OrderBy(x => x.Name).ThenBy(x => x.Id);
}
