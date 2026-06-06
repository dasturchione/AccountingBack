using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleDocTables;

public class SaleDocTableByIdCriteriaBuilder : ICriteriaBuilder<SaleDocTable, GetByIdOptions<long>>
{
    public Expression<Func<SaleDocTable, bool>> Build(GetByIdOptions<long> options) => x => x.Id == options.Id;
}
