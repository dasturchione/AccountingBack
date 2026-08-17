using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Branches;

public sealed class BranchListDtoOrderByBuilder : IOrderByBuilder<Branch, BranchListDto>
{
    public Func<IQueryable<BranchListDto>, IOrderedQueryable<BranchListDto>> Build() =>
        query => query.OrderBy(x => x.Name).ThenBy(x => x.Id);
}
