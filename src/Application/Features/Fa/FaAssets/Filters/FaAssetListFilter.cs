using SharedKernel.Filters;

namespace Application.Features.FaAssets;

public class FaAssetListFilter : ISearchFilter, IPaginationFilter
{
    public int? FaGroupId { get; set; }
    public short? StatusId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
