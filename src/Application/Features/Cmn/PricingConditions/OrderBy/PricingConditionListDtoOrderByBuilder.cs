using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.PricingConditions;

public sealed class PricingConditionListDtoOrderByBuilder : IOrderByBuilder<PricingCondition, PricingConditionListDto>
{
    public Func<IQueryable<PricingConditionListDto>, IOrderedQueryable<PricingConditionListDto>> Build() =>
        query => query.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id);
}
