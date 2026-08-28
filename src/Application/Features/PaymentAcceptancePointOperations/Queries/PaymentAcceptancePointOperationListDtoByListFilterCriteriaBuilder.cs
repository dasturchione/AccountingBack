using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationListDtoByListFilterCriteriaBuilder
    : ICriteriaBuilder<PaymentAcceptancePointOperationListDto, PaymentAcceptancePointOperationListFilter>
{
    public Expression<Func<PaymentAcceptancePointOperationListDto, bool>> Build(PaymentAcceptancePointOperationListFilter filter) =>
        x => string.IsNullOrWhiteSpace(filter.Search) ||
             x.DocNumber.ToLower().Contains(filter.Search.ToLower()) ||
             x.PaymentAcceptancePointCode.ToLower().Contains(filter.Search.ToLower()) ||
             x.PaymentAcceptancePointName.ToLower().Contains(filter.Search.ToLower()) ||
             (x.ExternalTransactionNumber != null &&
              x.ExternalTransactionNumber.ToLower().Contains(filter.Search.ToLower()));
}
