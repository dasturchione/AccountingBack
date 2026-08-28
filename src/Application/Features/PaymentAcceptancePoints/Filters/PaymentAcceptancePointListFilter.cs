using SharedKernel.Filters;

namespace Application.Features.PaymentAcceptancePoints;

public class PaymentAcceptancePointListFilter : ISearchFilter, IPaginationFilter
{
    public short? TypeId { get; set; }
    public int? BankAccountId { get; set; }
    public short? StateId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
