using SharedKernel.Filters;

namespace Application.Features.CashFiscalTransfers;

public sealed class CashFiscalTransferListFilter : ISearchFilter, IPaginationFilter
{
    public int? FiscalCashRegisterId { get; set; }
    public int? CashBoxId { get; set; }
    public short? DirectionId { get; set; }
    public short? CurrencyId { get; set; }
    public short? StatusId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
