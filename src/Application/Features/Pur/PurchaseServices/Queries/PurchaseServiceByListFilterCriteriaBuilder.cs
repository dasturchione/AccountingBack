using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseServices;

public class PurchaseServiceByListFilterCriteriaBuilder : ICriteriaBuilder<PurchaseService, PurchaseServiceListFilter>
{
    public Expression<Func<PurchaseService, bool>> Build(PurchaseServiceListFilter options) =>
        x => (!options.ServiceTypeId.HasValue || x.ServiceTypeId == options.ServiceTypeId.Value) &&
             (!options.StateId.HasValue || x.StateId == options.StateId.Value);
}
