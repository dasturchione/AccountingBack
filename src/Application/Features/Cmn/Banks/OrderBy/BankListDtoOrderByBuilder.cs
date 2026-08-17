using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Banks;

public sealed class BankListDtoOrderByBuilder : IOrderByBuilder<Bank, BankListDto>
{
    public Func<IQueryable<BankListDto>, IOrderedQueryable<BankListDto>> Build() =>
        query => query.OrderBy(x => x.Name).ThenBy(x => x.Id);
}
