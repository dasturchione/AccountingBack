using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationByListFilterCriteriaBuilder
    : ICriteriaBuilder<PaymentAcceptancePointOperation, PaymentAcceptancePointOperationListFilter>
{
    public Expression<Func<PaymentAcceptancePointOperation, bool>> Build(PaymentAcceptancePointOperationListFilter filter) =>
        x => x.StateId == StateIdConst.ACTIVE &&
             (!filter.PaymentAcceptancePointId.HasValue || x.PaymentAcceptancePointId == filter.PaymentAcceptancePointId.Value) &&
             (!filter.DirectionId.HasValue || x.DirectionId == filter.DirectionId.Value) &&
             (!filter.CurrencyId.HasValue || x.CurrencyId == filter.CurrencyId.Value) &&
             (!filter.StatusId.HasValue || x.StatusId == filter.StatusId.Value) &&
             (!filter.RelatedDocumentId.HasValue || x.RelatedDocumentId == filter.RelatedDocumentId.Value) &&
             (!filter.DateFrom.HasValue || x.DocDate >= filter.DateFrom.Value) &&
             (!filter.DateTo.HasValue || x.DocDate <= filter.DateTo.Value);
}
