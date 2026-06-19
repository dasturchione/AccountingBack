using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashBoxes;

public class CashBoxByListFilterCriteriaBuilder : ICriteriaBuilder<CashBox, CashBoxListFilter>
{
    public Expression<Func<CashBox, bool>> Build(CashBoxListFilter options) =>
        x => (!options.BranchId.HasValue || x.BranchId == options.BranchId.Value);
}
