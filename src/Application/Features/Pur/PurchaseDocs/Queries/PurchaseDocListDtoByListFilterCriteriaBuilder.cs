using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<PurchaseDocListDto, PurchaseDocListFilter>
{
    public Expression<Func<PurchaseDocListDto, bool>> Build(PurchaseDocListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.DocNumber.ToLower().Contains(options.Search.ToLower());
}
