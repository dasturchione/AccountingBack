using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.FaDisposals;

public sealed class FaDisposalListDtoOrderByBuilder : IOrderByBuilder<FaDisposalDoc, FaDisposalListDto>
{
    public Func<IQueryable<FaDisposalListDto>, IOrderedQueryable<FaDisposalListDto>> Build() =>
        query => query.OrderByDescending(x => x.DisposalDate).ThenByDescending(x => x.Id);
}
