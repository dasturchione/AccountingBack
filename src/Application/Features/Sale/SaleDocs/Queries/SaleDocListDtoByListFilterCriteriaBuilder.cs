using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleDocs;

public class SaleDocListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<SaleDocListDto, SaleDocListFilter>
{
    public Expression<Func<SaleDocListDto, bool>> Build(SaleDocListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.DocNumber.ToLower().Contains(options.Search.ToLower());
}
