using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.FaDepreciations;

public sealed class FaDepreciationRunListDtoOrderByBuilder : IOrderByBuilder<FaDepreciationRun, FaDepreciationRunListDto>
{
    public Func<IQueryable<FaDepreciationRunListDto>, IOrderedQueryable<FaDepreciationRunListDto>> Build() =>
        query => query.OrderByDescending(x => x.PeriodMonth).ThenByDescending(x => x.Id);
}
