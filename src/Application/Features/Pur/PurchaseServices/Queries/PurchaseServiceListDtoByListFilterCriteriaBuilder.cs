using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseServices;

public class PurchaseServiceListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<PurchaseServiceListDto, PurchaseServiceListFilter>
{
    public Expression<Func<PurchaseServiceListDto, bool>> Build(PurchaseServiceListFilter options) =>
        x => string.IsNullOrEmpty(options.Search) ||
             x.Name.ToLower().Contains(options.Search.ToLower()) ||
             x.ServiceTypeName.ToLower().Contains(options.Search.ToLower()) ||
             x.AccountName.ToLower().Contains(options.Search.ToLower());
}
