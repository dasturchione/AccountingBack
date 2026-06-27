using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleConditions;

public class SaleConditionListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<SaleConditionListDto, SaleConditionListFilter>
{
    public Expression<Func<SaleConditionListDto, bool>> Build(SaleConditionListFilter options) =>
        x => string.IsNullOrEmpty(options.Search) ||
             x.CostingMethodName.ToLower().Contains(options.Search.ToLower()) ||
             x.CostingMethodCode.ToLower().Contains(options.Search.ToLower()) ||
             x.VatRateName.ToLower().Contains(options.Search.ToLower()) ||
             x.VatRateCode.ToLower().Contains(options.Search.ToLower());
}
