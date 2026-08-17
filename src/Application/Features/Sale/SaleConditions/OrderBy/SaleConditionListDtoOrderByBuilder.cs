using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.SaleConditions;

public sealed class SaleConditionListDtoOrderByBuilder : IOrderByBuilder<SaleCondition, SaleConditionListDto>
{
    public Func<IQueryable<SaleConditionListDto>, IOrderedQueryable<SaleConditionListDto>> Build() =>
        query => query.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id);
}
