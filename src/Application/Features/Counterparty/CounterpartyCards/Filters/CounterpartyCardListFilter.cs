using SharedKernel.Filters;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardListFilter : ISearchFilter, IPaginationFilter
{
    public short? CounterpartyTypeId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
