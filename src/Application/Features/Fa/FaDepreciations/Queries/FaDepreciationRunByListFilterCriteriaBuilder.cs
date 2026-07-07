using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaDepreciations;

public class FaDepreciationRunByListFilterCriteriaBuilder : ICriteriaBuilder<FaDepreciationRun, FaDepreciationRunListFilter>
{
    public Expression<Func<FaDepreciationRun, bool>> Build(FaDepreciationRunListFilter options) =>
        x => (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (!options.PeriodFrom.HasValue || x.PeriodMonth >= options.PeriodFrom.Value) &&
             (!options.PeriodTo.HasValue || x.PeriodMonth <= options.PeriodTo.Value);
}
