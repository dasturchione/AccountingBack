using SharedKernel.Filters;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxListFilter : ISearchFilter, IPaginationFilter
{
    public short? StateId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
