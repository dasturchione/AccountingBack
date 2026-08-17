using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Pay.Payments;

public sealed class PayrollPaymentListDtoOrderByBuilder : IOrderByBuilder<PayPaymentBatch, PayrollPaymentListDto>
{
    public Func<IQueryable<PayrollPaymentListDto>, IOrderedQueryable<PayrollPaymentListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
