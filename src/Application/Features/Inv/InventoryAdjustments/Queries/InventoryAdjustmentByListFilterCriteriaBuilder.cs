using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentByListFilterCriteriaBuilder : ICriteriaBuilder<InventoryAdjustmentDoc, InventoryAdjustmentListFilter>
{
    private readonly IUserContext _userContext;

    public InventoryAdjustmentByListFilterCriteriaBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Expression<Func<InventoryAdjustmentDoc, bool>> Build(InventoryAdjustmentListFilter options) =>
        x => (!_userContext.OrganizationId.HasValue || x.OrganizationId == _userContext.OrganizationId.Value) &&
             (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (string.IsNullOrWhiteSpace(options.AdjustmentType) || x.AdjustmentType == options.AdjustmentType) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo) &&
             (string.IsNullOrWhiteSpace(options.Search) || x.DocNumber.Contains(options.Search));
}
