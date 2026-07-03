using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryCounts;

public class InventoryCountByListFilterCriteriaBuilder : ICriteriaBuilder<InventoryCountDoc, InventoryCountListFilter>
{
    private readonly IUserContext _userContext;

    public InventoryCountByListFilterCriteriaBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Expression<Func<InventoryCountDoc, bool>> Build(InventoryCountListFilter options) =>
        x =>
            (!_userContext.OrganizationId.HasValue || x.OrganizationId == _userContext.OrganizationId.Value) &&
            (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
            (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
            (string.IsNullOrWhiteSpace(options.Search) ||
             x.DocNumber.ToLower().Contains(options.Search.ToLower()));
}
