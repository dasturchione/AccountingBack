using SharedKernel.Filters;

namespace Application.Features.BankTerminals;

public class BankTerminalListFilter : ISearchFilter, IPaginationFilter
{
    public int? BankAccountId { get; set; }
    public short? StateId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
