using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.CashOperations;

public sealed class CashOperationListDtoOrderByBuilder : IOrderByBuilder<CashOperation, CashOperationListDto>
{
    public Func<IQueryable<CashOperationListDto>, IOrderedQueryable<CashOperationListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
