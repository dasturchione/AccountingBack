using SharedKernel.Filters;

namespace Application.Features.Platform;

public class PlatformOrganizationListFilter : IPaginationFilter
{
    public string? Search { get; set; }
    public int? RegionId { get; set; }
    public short? StateId { get; set; }
    public string? SetupStatus { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
}
