using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Warehouses;

public class WarehouseByIdCriteriaBuilder : ICriteriaBuilder<Warehouse, GetByIdOptions<int>>
{
    public Expression<Func<Warehouse, bool>> Build(GetByIdOptions<int> options) => x => x.Id == options.Id;
}
