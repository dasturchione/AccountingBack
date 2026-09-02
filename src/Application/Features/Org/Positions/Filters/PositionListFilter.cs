using SharedKernel.Filters;

namespace Application.Features.Positions;

public class PositionListFilter : ISearchFilter, IPaginationFilter
{
    internal int? OrganizationId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
