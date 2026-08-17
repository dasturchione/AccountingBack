using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Pay.Components;

public sealed class PayrollComponentListDtoOrderByBuilder : IOrderByBuilder<PayComponent, PayrollComponentListDto>
{
    public Func<IQueryable<PayrollComponentListDto>, IOrderedQueryable<PayrollComponentListDto>> Build() =>
        query => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Code).ThenBy(x => x.Id);
}
