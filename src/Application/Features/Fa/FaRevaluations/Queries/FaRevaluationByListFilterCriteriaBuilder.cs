using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaRevaluations;

public class FaRevaluationByListFilterCriteriaBuilder : ICriteriaBuilder<FaRevaluationDoc, FaRevaluationListFilter>
{
    public Expression<Func<FaRevaluationDoc, bool>> Build(FaRevaluationListFilter options) =>
        x => (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (!options.DateFrom.HasValue || x.RevaluationDate >= options.DateFrom.Value) &&
             (!options.DateTo.HasValue || x.RevaluationDate <= options.DateTo.Value);
}
