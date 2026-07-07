using SharedKernel.Filters;

namespace Application.Features.FaDepreciations;

public class FaDepreciationRunListFilter : ISearchFilter, IPaginationFilter
{
    public short? StatusId { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
