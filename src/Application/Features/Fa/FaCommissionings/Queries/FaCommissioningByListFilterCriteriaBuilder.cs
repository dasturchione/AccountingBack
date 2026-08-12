using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaCommissionings;

public class FaCommissioningByListFilterCriteriaBuilder :
    ICriteriaBuilder<FaCommissioningDoc, FaCommissioningListFilter>
{
    public Expression<Func<FaCommissioningDoc, bool>> Build(FaCommissioningListFilter options) =>
        document =>
            (!options.StatusId.HasValue || document.StatusId == options.StatusId.Value) &&
            (!options.DateFrom.HasValue || document.DocDate >= options.DateFrom.Value) &&
            (!options.DateTo.HasValue || document.DocDate <= options.DateTo.Value);
}
