using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Cmn.CurrencyRevaluations;

public sealed class CurrencyRevaluationListDtoOrderByBuilder : IOrderByBuilder<CurrencyRevaluation, CurrencyRevaluationListDto>
{
    public Func<IQueryable<CurrencyRevaluationListDto>, IOrderedQueryable<CurrencyRevaluationListDto>> Build() =>
        query => query.OrderByDescending(x => x.RevaluationDate).ThenByDescending(x => x.Id);
}
