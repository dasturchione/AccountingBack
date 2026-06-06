using SharedKernel.Filters;

namespace Application.Features.ChartAccounts;

public class ChartAccountListFilter : ISearchFilter, IPaginationFilter
{
    public int? OrganizationId { get; set; }
    public int? ParentId { get; set; }
    public bool? IsGroup { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
