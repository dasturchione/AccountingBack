using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Contracts;

public sealed class ContractListDtoOrderByBuilder : IOrderByBuilder<Contract, ContractListDto>
{
    public Func<IQueryable<ContractListDto>, IOrderedQueryable<ContractListDto>> Build() =>
        query => query.OrderByDescending(x => x.ContractDate).ThenByDescending(x => x.Id);
}
