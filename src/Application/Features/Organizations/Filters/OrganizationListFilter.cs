using SharedKernel.Filters;

namespace Application.Features.Organizations;

public class OrganizationListFilter : ISearchFilter, IPaginationFilter
{
    public int? RegionId { get; set; }
    public bool? IsParent { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
