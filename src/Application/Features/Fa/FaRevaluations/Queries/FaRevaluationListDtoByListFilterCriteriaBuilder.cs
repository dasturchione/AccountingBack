using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaRevaluations;

public class FaRevaluationListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<FaRevaluationListDto, FaRevaluationListFilter>
{
    public Expression<Func<FaRevaluationListDto, bool>> Build(FaRevaluationListFilter options) =>
        x => (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (!options.DateFrom.HasValue || x.RevaluationDate >= options.DateFrom.Value) &&
             (!options.DateTo.HasValue || x.RevaluationDate <= options.DateTo.Value) &&
             (string.IsNullOrWhiteSpace(options.Search) ||
              x.DocNumber.ToLower().Contains(options.Search.ToLower()) ||
              x.StatusName.ToLower().Contains(options.Search.ToLower()) ||
              (x.Reason != null && x.Reason.ToLower().Contains(options.Search.ToLower())));
}
