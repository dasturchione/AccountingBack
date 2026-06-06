using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Warehouses;

public class WarehouseByListFilterCriteriaBuilder : ICriteriaBuilder<Warehouse, WarehouseListFilter>
{
    public Expression<Func<Warehouse, bool>> Build(WarehouseListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.BranchId.HasValue || x.BranchId == options.BranchId.Value);
}
