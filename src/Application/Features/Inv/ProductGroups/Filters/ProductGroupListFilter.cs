using SharedKernel.Filters;

namespace Application.Features.ProductGroups;

public class ProductGroupListFilter : ISearchFilter, IPaginationFilter
{
    internal int? OrganizationId { get; set; }
    public bool? IsService { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
