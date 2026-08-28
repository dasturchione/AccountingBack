using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationListDtoOrderByBuilder
    : IOrderByBuilder<PaymentAcceptancePointOperation, PaymentAcceptancePointOperationListDto>
{
    public Func<IQueryable<PaymentAcceptancePointOperationListDto>, IOrderedQueryable<PaymentAcceptancePointOperationListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
