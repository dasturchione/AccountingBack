using SharedKernel.Filters;

namespace Application.Features.Platform;

public class PlatformUserListFilter : IPaginationFilter
{
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public int? RoleId { get; set; }
    public short? UserKindId { get; set; }
    public short? StateId { get; set; }
    public int? OrganizationId { get; set; }
    public bool? HasGlobalAccess { get; set; }
}