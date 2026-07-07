using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaDepreciations;

public class FaDepreciationRunListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<FaDepreciationRunListDto, FaDepreciationRunListFilter>
{
    public Expression<Func<FaDepreciationRunListDto, bool>> Build(FaDepreciationRunListFilter options) =>
        x => (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (!options.PeriodFrom.HasValue || x.PeriodMonth >= options.PeriodFrom.Value) &&
             (!options.PeriodTo.HasValue || x.PeriodMonth <= options.PeriodTo.Value) &&
             (string.IsNullOrWhiteSpace(options.Search) ||
              x.DocNumber.ToLower().Contains(options.Search.ToLower()) ||
              x.StatusName.ToLower().Contains(options.Search.ToLower()) ||
              (x.Note != null && x.Note.ToLower().Contains(options.Search.ToLower())));
}
