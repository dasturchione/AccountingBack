using SharedKernel.Filters;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableListFilter : ISearchFilter, IPaginationFilter
{
    public long? OwnerId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
