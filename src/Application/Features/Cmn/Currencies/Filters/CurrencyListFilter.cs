using SharedKernel.Filters;

namespace Application.Features.Cmn.Currencies;

public sealed class CurrencyListFilter : ISearchFilter, IPaginationFilter
{
    public short? StateId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
