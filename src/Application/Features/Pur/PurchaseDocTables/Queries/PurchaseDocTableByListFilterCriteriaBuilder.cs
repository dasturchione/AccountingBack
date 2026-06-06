using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableByListFilterCriteriaBuilder : ICriteriaBuilder<PurchaseDocTable, PurchaseDocTableListFilter>
{
    public Expression<Func<PurchaseDocTable, bool>> Build(PurchaseDocTableListFilter options) =>
        x => (!options.OwnerId.HasValue || x.OwnerId == options.OwnerId.Value);
}
