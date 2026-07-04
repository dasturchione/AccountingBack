using SharedKernel.Filters;

namespace Application.Features.Cmn.CurrencyRates;

public sealed class CurrencyRateListFilter : ISearchFilter, IPaginationFilter, ISortFilter
{
    public short? BaseCurrencyId { get; set; }
    public short? TargetCurrencyId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
