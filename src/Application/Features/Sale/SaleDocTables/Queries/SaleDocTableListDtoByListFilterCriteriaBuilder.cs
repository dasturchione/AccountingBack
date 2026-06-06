using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleDocTables;

public class SaleDocTableListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<SaleDocTableListDto, SaleDocTableListFilter>
{
    public Expression<Func<SaleDocTableListDto, bool>> Build(SaleDocTableListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.ProductName.ToLower().Contains(options.Search.ToLower());
}
