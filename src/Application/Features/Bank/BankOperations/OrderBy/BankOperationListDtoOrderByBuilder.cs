using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.BankOperations;

public sealed class BankOperationListDtoOrderByBuilder : IOrderByBuilder<BankOperation, BankOperationListDto>
{
    public Func<IQueryable<BankOperationListDto>, IOrderedQueryable<BankOperationListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
