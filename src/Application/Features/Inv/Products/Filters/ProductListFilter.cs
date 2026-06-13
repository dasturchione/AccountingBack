using SharedKernel.Filters;

namespace Application.Features.Products;

public class ProductListFilter : ISearchFilter, IPaginationFilter
{
    public int? ProductGroupId { get; set; }
    public bool? IsService { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
