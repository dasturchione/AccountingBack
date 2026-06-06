using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashOperations;

public class CashOperationListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<CashOperationListDto, CashOperationListFilter>
{
    public Expression<Func<CashOperationListDto, bool>> Build(CashOperationListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.DocNumber.ToLower().Contains(options.Search.ToLower());
}
