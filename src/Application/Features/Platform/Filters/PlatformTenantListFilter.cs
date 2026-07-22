using SharedKernel.Filters;

namespace Application.Features.Platform
{
    public class PlatformTenantListFilter : IPaginationFilter
    {
        public string? Search { get; set; }
        public short? StateId { get; set; }
        public int Page { get; set; } = 1;
        public int? PageSize { get; set; } = 50;
    }
}
