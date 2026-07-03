using SharedKernel.Filters;

namespace Application.Features.Ledger;

public class LedgerFilter : IPaginationFilter
{
    public int AccountId { get; set; }
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public int? CounterpartyId { get; set; }
    public int? WarehouseId { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
}
