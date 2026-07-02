using SharedKernel.Filters;

namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentListFilter : ISearchFilter, IPaginationFilter
{
    public int? WarehouseId { get; set; }
    public short? StatusId { get; set; }
    public string? AdjustmentType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
