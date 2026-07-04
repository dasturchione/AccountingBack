using SharedKernel.Filters;

namespace Application.Features.Cmn.CurrencyRevaluations;

public sealed class CurrencyRevaluationListFilter : ISearchFilter, IPaginationFilter, ISortFilter
{
    public DateTime? RevaluationFrom { get; set; }
    public DateTime? RevaluationTo { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
