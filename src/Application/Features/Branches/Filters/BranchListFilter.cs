using SharedKernel.Filters;

namespace Application.Features.Branches;

public class BranchListFilter : ISearchFilter, IPaginationFilter
{
    public int? OrganizationId { get; set; }
    public int? RegionId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
