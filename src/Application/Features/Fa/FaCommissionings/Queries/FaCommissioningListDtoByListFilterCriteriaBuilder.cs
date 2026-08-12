using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaCommissionings;

public class FaCommissioningListDtoByListFilterCriteriaBuilder :
    ICriteriaBuilder<FaCommissioningListDto, FaCommissioningListFilter>
{
    public Expression<Func<FaCommissioningListDto, bool>> Build(FaCommissioningListFilter options) =>
        document =>
            (!options.StatusId.HasValue || document.StatusId == options.StatusId.Value) &&
            (!options.DateFrom.HasValue || document.DocDate >= options.DateFrom.Value) &&
            (!options.DateTo.HasValue || document.DocDate <= options.DateTo.Value) &&
            (string.IsNullOrWhiteSpace(options.Search) ||
             document.DocNumber.ToLower().Contains(options.Search.ToLower()) ||
             document.StatusName.ToLower().Contains(options.Search.ToLower()) ||
             (document.Note != null && document.Note.ToLower().Contains(options.Search.ToLower())));
}
