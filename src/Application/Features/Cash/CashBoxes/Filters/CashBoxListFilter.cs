using SharedKernel.Filters;

namespace Application.Features.CashBoxes;

public class CashBoxListFilter : ISearchFilter, IPaginationFilter
{
    public int? BranchId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
