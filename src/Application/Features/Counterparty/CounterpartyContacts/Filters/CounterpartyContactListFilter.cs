using SharedKernel.Filters;

namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactListFilter : ISearchFilter, IPaginationFilter
{
    public int? CounterpartyId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
