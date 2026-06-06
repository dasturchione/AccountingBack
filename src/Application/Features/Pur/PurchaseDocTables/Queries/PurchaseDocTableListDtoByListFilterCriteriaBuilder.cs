using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<PurchaseDocTableListDto, PurchaseDocTableListFilter>
{
    public Expression<Func<PurchaseDocTableListDto, bool>> Build(PurchaseDocTableListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.ProductName.ToLower().Contains(options.Search.ToLower());
}
