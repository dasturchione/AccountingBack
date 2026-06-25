using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<PurchaseDocTableListDto, PurchaseDocTableListFilter>
{
    public Expression<Func<PurchaseDocTableListDto, bool>> Build(PurchaseDocTableListFilter options)
    {
        var search = options.Search?.ToLower() ?? string.Empty;

        return x => search.Length == 0 ||
                    (x.ProductName != null && x.ProductName.ToLower().Contains(search));
    }
}
