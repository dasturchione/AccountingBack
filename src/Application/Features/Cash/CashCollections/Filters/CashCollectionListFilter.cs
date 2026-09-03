using SharedKernel.Filters;

namespace Application.Features.CashCollections;

public sealed class CashCollectionListFilter : ISearchFilter, IPaginationFilter
{
    public int? CashBoxId { get; set; }
    public int? BankAccountId { get; set; }
    public short? CurrencyId { get; set; }
    public short? StatusId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
