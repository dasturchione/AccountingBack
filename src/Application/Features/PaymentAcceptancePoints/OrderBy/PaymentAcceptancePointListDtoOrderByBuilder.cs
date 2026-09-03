using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.PaymentAcceptancePoints;

public sealed class PaymentAcceptancePointListDtoOrderByBuilder : IOrderByBuilder<PaymentAcceptancePoint, PaymentAcceptancePointListDto>
{
    public Func<IQueryable<PaymentAcceptancePointListDto>, IOrderedQueryable<PaymentAcceptancePointListDto>> Build() =>
        query => query.OrderBy(x => x.Name).ThenBy(x => x.Id);
}
