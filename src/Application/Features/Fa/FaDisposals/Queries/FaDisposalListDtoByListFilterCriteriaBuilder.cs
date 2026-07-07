using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaDisposals;

public class FaDisposalListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<FaDisposalListDto, FaDisposalListFilter>
{
    public Expression<Func<FaDisposalListDto, bool>> Build(FaDisposalListFilter options) =>
        x => (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (string.IsNullOrWhiteSpace(options.DisposalType) || x.DisposalType == options.DisposalType) &&
             (!options.DateFrom.HasValue || x.DisposalDate >= options.DateFrom.Value) &&
             (!options.DateTo.HasValue || x.DisposalDate <= options.DateTo.Value) &&
             (string.IsNullOrWhiteSpace(options.Search) ||
              x.DocNumber.ToLower().Contains(options.Search.ToLower()) ||
              x.StatusName.ToLower().Contains(options.Search.ToLower()) ||
              x.DisposalType.ToLower().Contains(options.Search.ToLower()) ||
              (x.Reason != null && x.Reason.ToLower().Contains(options.Search.ToLower())));
}
