using SharedKernel.Filters;

namespace Application.Features.SaleDocTables;

public class SaleDocTableListFilter : ISearchFilter, IPaginationFilter
{
    public long? OwnerId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
