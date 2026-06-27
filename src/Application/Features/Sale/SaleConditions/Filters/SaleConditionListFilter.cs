using SharedKernel.Filters;

namespace Application.Features.SaleConditions;

public class SaleConditionListFilter : ISearchFilter, IPaginationFilter
{
    public short? CostingMethodId { get; set; }
    public short? VatRateId { get; set; }
    public short? StateId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
