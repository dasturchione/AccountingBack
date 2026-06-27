using SharedKernel.Filters;

namespace Application.Features.PricingConditions;

public class PricingConditionListFilter : ISearchFilter, IPaginationFilter
{
    public short? PricingMethodId { get; set; }
    public short? RoundingMethodId { get; set; }
    public short? StateId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
