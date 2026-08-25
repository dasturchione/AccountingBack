using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<InventoryAdjustmentListDto, InventoryAdjustmentListFilter>
{
    public Expression<Func<InventoryAdjustmentListDto, bool>> Build(InventoryAdjustmentListFilter options) =>
        x => (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (string.IsNullOrWhiteSpace(options.AdjustmentType) || x.AdjustmentType == options.AdjustmentType) &&
             (!options.DirectionId.HasValue || x.DirectionId == options.DirectionId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo) &&
             (string.IsNullOrWhiteSpace(options.Search) || x.DocNumber.Contains(options.Search));
}
