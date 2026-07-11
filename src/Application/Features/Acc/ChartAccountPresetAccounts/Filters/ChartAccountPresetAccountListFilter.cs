using SharedKernel.Filters;

namespace Application.Features.ChartAccountPresetAccounts;

public class ChartAccountPresetAccountListFilter : ISearchFilter, IPaginationFilter
{
    public short? PresetId { get; set; }
    //public int? ParentPresetAccountId { get; set; }
    //public bool? IsGroup { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
