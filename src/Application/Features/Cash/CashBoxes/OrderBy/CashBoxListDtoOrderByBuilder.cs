using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.CashBoxes;

public sealed class CashBoxListDtoOrderByBuilder : IOrderByBuilder<CashBox, CashBoxListDto>
{
    public Func<IQueryable<CashBoxListDto>, IOrderedQueryable<CashBoxListDto>> Build() =>
        query => query.OrderByDescending(x => x.IsMain).ThenBy(x => x.Name).ThenBy(x => x.Id);
}
