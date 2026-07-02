using SharedKernel.Filters;

namespace Application.Features.Inv.ProductStocks;

public class ProductStockFilter : IPaginationFilter
{
    public int? WarehouseId { get; set; }
    public int? ProductGroupId { get; set; }
    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public int? PageSize { get; set; }
}
