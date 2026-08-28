using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.BankOperations;

public class BankOperationListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<BankOperationListDto, BankOperationListFilter>
{
    public Expression<Func<BankOperationListDto, bool>> Build(BankOperationListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.DocNumber.ToLower().Contains(options.Search.ToLower()) ||
                (x.BankDocumentNumber != null &&
                 x.BankDocumentNumber.ToLower().Contains(options.Search.ToLower())) ||
                (x.CashCollectionDocNumber != null &&
                 x.CashCollectionDocNumber.ToLower().Contains(options.Search.ToLower()));
}
