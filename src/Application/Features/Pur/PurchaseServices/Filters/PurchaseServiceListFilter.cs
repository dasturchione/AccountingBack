using SharedKernel.Filters;

namespace Application.Features.PurchaseServices;

public class PurchaseServiceListFilter : ISearchFilter, IPaginationFilter
{
    public int? ServiceTypeId { get; set; }
    public short? StateId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
