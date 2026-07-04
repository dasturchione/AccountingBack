using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<TaxListDto, TaxListFilter>
{
    public Expression<Func<TaxListDto, bool>> Build(TaxListFilter options) =>
        x => string.IsNullOrWhiteSpace(options.Search) ||
             x.Code.ToLower().Contains(options.Search.ToLower()) ||
             x.Name.ToLower().Contains(options.Search.ToLower());
}
