using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PricingConditions;

public class PricingConditionListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<PricingConditionListDto, PricingConditionListFilter>
{
    public Expression<Func<PricingConditionListDto, bool>> Build(PricingConditionListFilter options) =>
        x => string.IsNullOrEmpty(options.Search) ||
             x.PricingMethodName.ToLower().Contains(options.Search.ToLower()) ||
             x.PricingMethodCode.ToLower().Contains(options.Search.ToLower()) ||
             x.RoundingMethodName.ToLower().Contains(options.Search.ToLower()) ||
             x.RoundingMethodCode.ToLower().Contains(options.Search.ToLower());
}
