using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.FaRevaluations;

public sealed class FaRevaluationListDtoOrderByBuilder : IOrderByBuilder<FaRevaluationDoc, FaRevaluationListDto>
{
    public Func<IQueryable<FaRevaluationListDto>, IOrderedQueryable<FaRevaluationListDto>> Build() =>
        query => query.OrderByDescending(x => x.RevaluationDate).ThenByDescending(x => x.Id);
}
