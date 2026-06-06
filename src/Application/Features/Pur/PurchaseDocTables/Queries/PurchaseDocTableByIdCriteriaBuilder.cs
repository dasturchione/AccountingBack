using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableByIdCriteriaBuilder : ICriteriaBuilder<PurchaseDocTable, GetByIdOptions<long>>
{
    public Expression<Func<PurchaseDocTable, bool>> Build(GetByIdOptions<long> options) => x => x.Id == options.Id;
}
