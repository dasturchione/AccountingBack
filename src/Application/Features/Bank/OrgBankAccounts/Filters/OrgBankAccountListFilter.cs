using SharedKernel.Filters;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountListFilter : ISearchFilter, IPaginationFilter
{
    public int? OrganizationId { get; set; }
    public int? BankId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
