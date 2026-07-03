using SharedKernel.Filters;

namespace Application.Features.InventoryCounts;

public class InventoryCountListFilter : ISearchFilter, IPaginationFilter
{
    public string? Search { get; set; }
    public int? WarehouseId { get; set; }
    public short? StatusId { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
