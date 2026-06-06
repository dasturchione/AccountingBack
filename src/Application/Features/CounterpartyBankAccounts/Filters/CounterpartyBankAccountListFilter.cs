using SharedKernel.Filters;

namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountListFilter : ISearchFilter, IPaginationFilter
{
    public int? OrganizationId { get; set; }
    public int? CounterpartyId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
