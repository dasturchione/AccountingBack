using SharedKernel.Filters;

namespace Application.Features.Inv.ProductPrices;

public class ProductPriceListFilter : ISearchFilter, IPaginationFilter
{
    internal int? OrganizationId { get; set; }
    public int? ProductId { get; set; }
    public short? PriceTypeId { get; set; }
    public short? UnitId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
