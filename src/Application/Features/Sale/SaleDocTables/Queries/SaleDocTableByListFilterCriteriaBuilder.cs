using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleDocTables;

public class SaleDocTableByListFilterCriteriaBuilder : ICriteriaBuilder<SaleDocTable, SaleDocTableListFilter>
{
    public Expression<Func<SaleDocTable, bool>> Build(SaleDocTableListFilter options) =>
        x => (!options.OwnerId.HasValue || x.OwnerId == options.OwnerId.Value);
}
